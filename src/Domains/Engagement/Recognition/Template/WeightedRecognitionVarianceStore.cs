namespace Acme.Workforce.Engagement.Recognition.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted recognition variances by code.</summary>
public interface IWeightedRecognitionVarianceStore
{
    WeightedRecognitionVariance? Find(string code);
    void Save(WeightedRecognitionVariance item);
    bool Remove(string code);
    List<WeightedRecognitionVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedRecognitionVarianceStore(Dictionary<string, WeightedRecognitionVariance> items, List<string> order) : IWeightedRecognitionVarianceStore
{
    public Dictionary<string, WeightedRecognitionVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedRecognitionVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedRecognitionVariance item)
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

    public List<WeightedRecognitionVariance> List()
    {
        var outValue = new List<WeightedRecognitionVariance>();
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
public sealed class CachedWeightedRecognitionVarianceStore(MemoryWeightedRecognitionVarianceStore inner, WeightedRecognitionVarianceCache cache, int hits, int misses) : IWeightedRecognitionVarianceStore
{
    public MemoryWeightedRecognitionVarianceStore Inner { get; set; } = inner;
    public WeightedRecognitionVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedRecognitionVariance? Find(string code)
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

    public void Save(WeightedRecognitionVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedRecognitionVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted recognition variances. Empty fields match all.</summary>
public sealed record WeightedRecognitionVarianceQuery(string Channel, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedRecognitionVariance item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedRecognitionVariance> Apply(List<WeightedRecognitionVariance> items)
    {
        var matched = Collections.Filter(items, (WeightedRecognitionVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted recognition variance store.</summary>
public static class WeightedRecognitionVarianceStore
{
    public static IWeightedRecognitionVarianceStore NewMemoryWeightedRecognitionVarianceStore()
    {
        return new MemoryWeightedRecognitionVarianceStore(
            new Dictionary<string, WeightedRecognitionVariance>(),
            new List<string>());
    }

    public static IWeightedRecognitionVarianceStore NewCachedWeightedRecognitionVarianceStore(MemoryWeightedRecognitionVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedRecognitionVarianceStore(
            inner,
            WeightedRecognitionVarianceCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<WeightedRecognitionVariance> FindWeightedRecognitionVariances(IWeightedRecognitionVarianceStore store, WeightedRecognitionVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
