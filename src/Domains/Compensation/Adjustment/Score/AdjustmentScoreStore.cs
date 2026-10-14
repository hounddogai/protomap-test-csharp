namespace Acme.Workforce.Compensation.Adjustment.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment scores by code.</summary>
public interface IAdjustmentScoreStore
{
    AdjustmentScore? Find(string code);
    void Save(AdjustmentScore item);
    bool Remove(string code);
    List<AdjustmentScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentScoreStore(Dictionary<string, AdjustmentScore> items, List<string> order) : IAdjustmentScoreStore
{
    public Dictionary<string, AdjustmentScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentScore item)
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

    public List<AdjustmentScore> List()
    {
        var outValue = new List<AdjustmentScore>();
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
public sealed class CachedAdjustmentScoreStore(MemoryAdjustmentScoreStore inner, AdjustmentScoreCache cache, int hits, int misses) : IAdjustmentScoreStore
{
    public MemoryAdjustmentScoreStore Inner { get; set; } = inner;
    public AdjustmentScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentScore? Find(string code)
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

    public void Save(AdjustmentScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment scores. Empty fields match all.</summary>
public sealed record AdjustmentScoreQuery(string Department, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentScore item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdjustmentScore> Apply(List<AdjustmentScore> items)
    {
        var matched = Collections.Filter(items, (AdjustmentScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment score store.</summary>
public static class AdjustmentScoreStore
{
    public static IAdjustmentScoreStore NewMemoryAdjustmentScoreStore()
    {
        return new MemoryAdjustmentScoreStore(new Dictionary<string, AdjustmentScore>(), new List<string>());
    }

    public static IAdjustmentScoreStore NewCachedAdjustmentScoreStore(MemoryAdjustmentScoreStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentScoreStore(inner, AdjustmentScoreCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AdjustmentScore> FindAdjustmentScores(IAdjustmentScoreStore store, AdjustmentScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
