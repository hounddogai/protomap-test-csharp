namespace Acme.Workforce.Engagement.Recognition.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered recognition scores by code.</summary>
public interface ITieredRecognitionScoreStore
{
    TieredRecognitionScore? Find(string code);
    void Save(TieredRecognitionScore item);
    bool Remove(string code);
    List<TieredRecognitionScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredRecognitionScoreStore(Dictionary<string, TieredRecognitionScore> items, List<string> order) : ITieredRecognitionScoreStore
{
    public Dictionary<string, TieredRecognitionScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredRecognitionScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredRecognitionScore item)
    {
        if (!Items.ContainsKey(item.Code))
        {
            Order.Add(item.Code);
        }
        Items[item.Code] = item;
    }

    public bool Remove(string code)
    {
        if (!Items.ContainsKey(code))
        {
            return false;
        }
        Items.Remove(code);
        var kept = new List<string>();
        foreach (var existing in Order)
        {
            if (existing != code)
            {
                kept.Add(existing);
            }
        }
        Order = kept;
        return true;
    }

    public List<TieredRecognitionScore> List()
    {
        var outValue = new List<TieredRecognitionScore>();
        foreach (var key in Order)
        {
            if (!Items.TryGetValue(key, out var found))
            {
                continue;
            }
            outValue.Add(found);
        }
        return outValue;
    }
}

/// <summary>Serves reads from a short-lived cache in front of another store.</summary>
public sealed class CachedTieredRecognitionScoreStore(MemoryTieredRecognitionScoreStore inner, TieredRecognitionScoreCache cache, int hits, int misses) : ITieredRecognitionScoreStore
{
    public MemoryTieredRecognitionScoreStore Inner { get; set; } = inner;
    public TieredRecognitionScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredRecognitionScore? Find(string code)
    {
        if (Cache.Get(code) is not { } hit)
        {
            Misses += 1;
            if (Inner.Find(code) is not { } found)
            {
                return null;
            }
            Cache.Put(code, found);
            return found;
        }
        Hits += 1;
        return hit;
    }

    public void Save(TieredRecognitionScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredRecognitionScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered recognition scores. Empty fields match all.</summary>
public sealed record TieredRecognitionScoreQuery(string Name, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(TieredRecognitionScore item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredRecognitionScore> Apply(List<TieredRecognitionScore> items)
    {
        var matched = Collections.Filter(items, (TieredRecognitionScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered recognition score store.</summary>
public static class TieredRecognitionScoreStore
{
    public static ITieredRecognitionScoreStore NewMemoryTieredRecognitionScoreStore()
    {
        return new MemoryTieredRecognitionScoreStore(
            new Dictionary<string, TieredRecognitionScore>(),
            new List<string>());
    }

    public static ITieredRecognitionScoreStore NewCachedTieredRecognitionScoreStore(MemoryTieredRecognitionScoreStore inner, int ttl, IClock clock)
    {
        return new CachedTieredRecognitionScoreStore(
            inner,
            TieredRecognitionScoreCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<TieredRecognitionScore> FindTieredRecognitionScores(ITieredRecognitionScoreStore store, TieredRecognitionScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
