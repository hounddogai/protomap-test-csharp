namespace Acme.Workforce.Contractors.Rate.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate estimates by code.</summary>
public interface IRateEstimateStore
{
    RateEstimate? Find(string code);
    void Save(RateEstimate item);
    bool Remove(string code);
    List<RateEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateEstimateStore(Dictionary<string, RateEstimate> items, List<string> order) : IRateEstimateStore
{
    public Dictionary<string, RateEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateEstimate item)
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

    public List<RateEstimate> List()
    {
        var outValue = new List<RateEstimate>();
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
public sealed class CachedRateEstimateStore(MemoryRateEstimateStore inner, RateEstimateCache cache, int hits, int misses) : IRateEstimateStore
{
    public MemoryRateEstimateStore Inner { get; set; } = inner;
    public RateEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateEstimate? Find(string code)
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

    public void Save(RateEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate estimates. Empty fields match all.</summary>
public sealed record RateEstimateQuery(string Channel, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(RateEstimate item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RateEstimate> Apply(List<RateEstimate> items)
    {
        var matched = Collections.Filter(items, (RateEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate estimate store.</summary>
public static class RateEstimateStore
{
    public static IRateEstimateStore NewMemoryRateEstimateStore()
    {
        return new MemoryRateEstimateStore(new Dictionary<string, RateEstimate>(), new List<string>());
    }

    public static IRateEstimateStore NewCachedRateEstimateStore(MemoryRateEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedRateEstimateStore(inner, RateEstimateCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RateEstimate> FindRateEstimates(IRateEstimateStore store, RateEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
