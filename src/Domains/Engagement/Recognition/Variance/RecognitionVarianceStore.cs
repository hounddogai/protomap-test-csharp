namespace Acme.Workforce.Engagement.Recognition.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition variances by code.</summary>
public interface IRecognitionVarianceStore
{
    RecognitionVariance? Find(string code);
    void Save(RecognitionVariance item);
    bool Remove(string code);
    List<RecognitionVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionVarianceStore(Dictionary<string, RecognitionVariance> items, List<string> order) : IRecognitionVarianceStore
{
    public Dictionary<string, RecognitionVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionVariance item)
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

    public List<RecognitionVariance> List()
    {
        var outValue = new List<RecognitionVariance>();
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
public sealed class CachedRecognitionVarianceStore(MemoryRecognitionVarianceStore inner, RecognitionVarianceCache cache, int hits, int misses) : IRecognitionVarianceStore
{
    public MemoryRecognitionVarianceStore Inner { get; set; } = inner;
    public RecognitionVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionVariance? Find(string code)
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

    public void Save(RecognitionVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition variances. Empty fields match all.</summary>
public sealed record RecognitionVarianceQuery(string Title, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionVariance item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RecognitionVariance> Apply(List<RecognitionVariance> items)
    {
        var matched = Collections.Filter(items, (RecognitionVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition variance store.</summary>
public static class RecognitionVarianceStore
{
    public static IRecognitionVarianceStore NewMemoryRecognitionVarianceStore()
    {
        return new MemoryRecognitionVarianceStore(new Dictionary<string, RecognitionVariance>(), new List<string>());
    }

    public static IRecognitionVarianceStore NewCachedRecognitionVarianceStore(MemoryRecognitionVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionVarianceStore(inner, RecognitionVarianceCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RecognitionVariance> FindRecognitionVariances(IRecognitionVarianceStore store, RecognitionVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
