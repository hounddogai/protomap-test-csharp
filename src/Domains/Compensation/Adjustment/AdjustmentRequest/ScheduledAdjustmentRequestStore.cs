namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled adjustment requests by code.</summary>
public interface IScheduledAdjustmentRequestStore
{
    ScheduledAdjustmentRequest? Find(string code);
    void Save(ScheduledAdjustmentRequest item);
    bool Remove(string code);
    List<ScheduledAdjustmentRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledAdjustmentRequestStore(Dictionary<string, ScheduledAdjustmentRequest> items, List<string> order) : IScheduledAdjustmentRequestStore
{
    public Dictionary<string, ScheduledAdjustmentRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledAdjustmentRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledAdjustmentRequest item)
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

    public List<ScheduledAdjustmentRequest> List()
    {
        var outValue = new List<ScheduledAdjustmentRequest>();
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
public sealed class CachedScheduledAdjustmentRequestStore(MemoryScheduledAdjustmentRequestStore inner, ScheduledAdjustmentRequestCache cache, int hits, int misses) : IScheduledAdjustmentRequestStore
{
    public MemoryScheduledAdjustmentRequestStore Inner { get; set; } = inner;
    public ScheduledAdjustmentRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledAdjustmentRequest? Find(string code)
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

    public void Save(ScheduledAdjustmentRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledAdjustmentRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled adjustment requests. Empty fields match all.</summary>
public sealed record ScheduledAdjustmentRequestQuery(string Region, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledAdjustmentRequest item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledAdjustmentRequest> Apply(List<ScheduledAdjustmentRequest> items)
    {
        var matched = Collections.Filter(items, (ScheduledAdjustmentRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled adjustment request store.</summary>
public static class ScheduledAdjustmentRequestStore
{
    public static IScheduledAdjustmentRequestStore NewMemoryScheduledAdjustmentRequestStore()
    {
        return new MemoryScheduledAdjustmentRequestStore(
            new Dictionary<string, ScheduledAdjustmentRequest>(),
            new List<string>());
    }

    public static IScheduledAdjustmentRequestStore NewCachedScheduledAdjustmentRequestStore(MemoryScheduledAdjustmentRequestStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledAdjustmentRequestStore(
            inner,
            ScheduledAdjustmentRequestCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledAdjustmentRequest> FindScheduledAdjustmentRequests(IScheduledAdjustmentRequestStore store, ScheduledAdjustmentRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
