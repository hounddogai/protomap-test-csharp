namespace Acme.Workforce.Engagement.Recognition.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition scores by code.</summary>
public interface IRecognitionScoreStore
{
    RecognitionScore? Find(string code);
    void Save(RecognitionScore item);
    bool Remove(string code);
    List<RecognitionScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionScoreStore(Dictionary<string, RecognitionScore> items, List<string> order) : IRecognitionScoreStore
{
    public Dictionary<string, RecognitionScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionScore item)
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

    public List<RecognitionScore> List()
    {
        var outValue = new List<RecognitionScore>();
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
public sealed class CachedRecognitionScoreStore(MemoryRecognitionScoreStore inner, RecognitionScoreCache cache, int hits, int misses) : IRecognitionScoreStore
{
    public MemoryRecognitionScoreStore Inner { get; set; } = inner;
    public RecognitionScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionScore? Find(string code)
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

    public void Save(RecognitionScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition scores. Empty fields match all.</summary>
public sealed record RecognitionScoreQuery(string Label, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionScore item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RecognitionScore> Apply(List<RecognitionScore> items)
    {
        var matched = Collections.Filter(items, (RecognitionScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition score store.</summary>
public static class RecognitionScoreStore
{
    public static IRecognitionScoreStore NewMemoryRecognitionScoreStore()
    {
        return new MemoryRecognitionScoreStore(new Dictionary<string, RecognitionScore>(), new List<string>());
    }

    public static IRecognitionScoreStore NewCachedRecognitionScoreStore(MemoryRecognitionScoreStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionScoreStore(inner, RecognitionScoreCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RecognitionScore> FindRecognitionScores(IRecognitionScoreStore store, RecognitionScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
