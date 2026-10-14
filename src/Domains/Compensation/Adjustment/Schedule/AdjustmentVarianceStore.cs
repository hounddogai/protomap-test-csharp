namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment variances by code.</summary>
public interface IAdjustmentVarianceStore
{
    AdjustmentVariance? Find(string code);
    void Save(AdjustmentVariance item);
    bool Remove(string code);
    List<AdjustmentVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentVarianceStore(Dictionary<string, AdjustmentVariance> items, List<string> order) : IAdjustmentVarianceStore
{
    public Dictionary<string, AdjustmentVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentVariance item)
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

    public List<AdjustmentVariance> List()
    {
        var outValue = new List<AdjustmentVariance>();
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
public sealed class CachedAdjustmentVarianceStore(MemoryAdjustmentVarianceStore inner, AdjustmentVarianceCache cache, int hits, int misses) : IAdjustmentVarianceStore
{
    public MemoryAdjustmentVarianceStore Inner { get; set; } = inner;
    public AdjustmentVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentVariance? Find(string code)
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

    public void Save(AdjustmentVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment variances. Empty fields match all.</summary>
public sealed record AdjustmentVarianceQuery(string Segment, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentVariance item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<AdjustmentVariance> Apply(List<AdjustmentVariance> items)
    {
        var matched = Collections.Filter(items, (AdjustmentVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment variance store.</summary>
public static class AdjustmentVarianceStore
{
    public static IAdjustmentVarianceStore NewMemoryAdjustmentVarianceStore()
    {
        return new MemoryAdjustmentVarianceStore(new Dictionary<string, AdjustmentVariance>(), new List<string>());
    }

    public static IAdjustmentVarianceStore NewCachedAdjustmentVarianceStore(MemoryAdjustmentVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentVarianceStore(inner, AdjustmentVarianceCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AdjustmentVariance> FindAdjustmentVariances(IAdjustmentVarianceStore store, AdjustmentVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
