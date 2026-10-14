namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling occupancy adjustments by code.</summary>
public interface IRollingOccupancyAdjustmentStore
{
    RollingOccupancyAdjustment? Find(string code);
    void Save(RollingOccupancyAdjustment item);
    bool Remove(string code);
    List<RollingOccupancyAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingOccupancyAdjustmentStore(Dictionary<string, RollingOccupancyAdjustment> items, List<string> order) : IRollingOccupancyAdjustmentStore
{
    public Dictionary<string, RollingOccupancyAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingOccupancyAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingOccupancyAdjustment item)
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

    public List<RollingOccupancyAdjustment> List()
    {
        var outValue = new List<RollingOccupancyAdjustment>();
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
public sealed class CachedRollingOccupancyAdjustmentStore(MemoryRollingOccupancyAdjustmentStore inner, RollingOccupancyAdjustmentCache cache, int hits, int misses) : IRollingOccupancyAdjustmentStore
{
    public MemoryRollingOccupancyAdjustmentStore Inner { get; set; } = inner;
    public RollingOccupancyAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingOccupancyAdjustment? Find(string code)
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

    public void Save(RollingOccupancyAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingOccupancyAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling occupancy adjustments. Empty fields match all.</summary>
public sealed record RollingOccupancyAdjustmentQuery(string Region, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(RollingOccupancyAdjustment item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<RollingOccupancyAdjustment> Apply(List<RollingOccupancyAdjustment> items)
    {
        var matched = Collections.Filter(items, (RollingOccupancyAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling occupancy adjustment store.</summary>
public static class RollingOccupancyAdjustmentStore
{
    public static IRollingOccupancyAdjustmentStore NewMemoryRollingOccupancyAdjustmentStore()
    {
        return new MemoryRollingOccupancyAdjustmentStore(
            new Dictionary<string, RollingOccupancyAdjustment>(),
            new List<string>());
    }

    public static IRollingOccupancyAdjustmentStore NewCachedRollingOccupancyAdjustmentStore(MemoryRollingOccupancyAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedRollingOccupancyAdjustmentStore(
            inner,
            RollingOccupancyAdjustmentCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<RollingOccupancyAdjustment> FindRollingOccupancyAdjustments(IRollingOccupancyAdjustmentStore store, RollingOccupancyAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
