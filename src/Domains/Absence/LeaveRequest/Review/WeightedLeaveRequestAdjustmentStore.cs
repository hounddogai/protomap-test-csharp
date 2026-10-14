namespace Acme.Workforce.Absence.LeaveRequest.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted leave request adjustments by code.</summary>
public interface IWeightedLeaveRequestAdjustmentStore
{
    WeightedLeaveRequestAdjustment? Find(string code);
    void Save(WeightedLeaveRequestAdjustment item);
    bool Remove(string code);
    List<WeightedLeaveRequestAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedLeaveRequestAdjustmentStore(Dictionary<string, WeightedLeaveRequestAdjustment> items, List<string> order) : IWeightedLeaveRequestAdjustmentStore
{
    public Dictionary<string, WeightedLeaveRequestAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedLeaveRequestAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedLeaveRequestAdjustment item)
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

    public List<WeightedLeaveRequestAdjustment> List()
    {
        var outValue = new List<WeightedLeaveRequestAdjustment>();
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
public sealed class CachedWeightedLeaveRequestAdjustmentStore(MemoryWeightedLeaveRequestAdjustmentStore inner, WeightedLeaveRequestAdjustmentCache cache, int hits, int misses) : IWeightedLeaveRequestAdjustmentStore
{
    public MemoryWeightedLeaveRequestAdjustmentStore Inner { get; set; } = inner;
    public WeightedLeaveRequestAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedLeaveRequestAdjustment? Find(string code)
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

    public void Save(WeightedLeaveRequestAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedLeaveRequestAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted leave request adjustments. Empty fields match all.</summary>
public sealed record WeightedLeaveRequestAdjustmentQuery(string Source, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedLeaveRequestAdjustment item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedLeaveRequestAdjustment> Apply(List<WeightedLeaveRequestAdjustment> items)
    {
        var matched = Collections.Filter(items, (WeightedLeaveRequestAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted leave request adjustment store.</summary>
public static class WeightedLeaveRequestAdjustmentStore
{
    public static IWeightedLeaveRequestAdjustmentStore NewMemoryWeightedLeaveRequestAdjustmentStore()
    {
        return new MemoryWeightedLeaveRequestAdjustmentStore(
            new Dictionary<string, WeightedLeaveRequestAdjustment>(),
            new List<string>());
    }

    public static IWeightedLeaveRequestAdjustmentStore NewCachedWeightedLeaveRequestAdjustmentStore(MemoryWeightedLeaveRequestAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedLeaveRequestAdjustmentStore(
            inner,
            WeightedLeaveRequestAdjustmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<WeightedLeaveRequestAdjustment> FindWeightedLeaveRequestAdjustments(IWeightedLeaveRequestAdjustmentStore store, WeightedLeaveRequestAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
