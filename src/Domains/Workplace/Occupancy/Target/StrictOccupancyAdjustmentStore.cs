namespace Acme.Workforce.Workplace.Occupancy.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict occupancy adjustments by code.</summary>
public interface IStrictOccupancyAdjustmentStore
{
    StrictOccupancyAdjustment? Find(string code);
    void Save(StrictOccupancyAdjustment item);
    bool Remove(string code);
    List<StrictOccupancyAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictOccupancyAdjustmentStore(Dictionary<string, StrictOccupancyAdjustment> items, List<string> order) : IStrictOccupancyAdjustmentStore
{
    public Dictionary<string, StrictOccupancyAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictOccupancyAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictOccupancyAdjustment item)
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

    public List<StrictOccupancyAdjustment> List()
    {
        var outValue = new List<StrictOccupancyAdjustment>();
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
public sealed class CachedStrictOccupancyAdjustmentStore(MemoryStrictOccupancyAdjustmentStore inner, StrictOccupancyAdjustmentCache cache, int hits, int misses) : IStrictOccupancyAdjustmentStore
{
    public MemoryStrictOccupancyAdjustmentStore Inner { get; set; } = inner;
    public StrictOccupancyAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictOccupancyAdjustment? Find(string code)
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

    public void Save(StrictOccupancyAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictOccupancyAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict occupancy adjustments. Empty fields match all.</summary>
public sealed record StrictOccupancyAdjustmentQuery(string Department, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(StrictOccupancyAdjustment item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictOccupancyAdjustment> Apply(List<StrictOccupancyAdjustment> items)
    {
        var matched = Collections.Filter(items, (StrictOccupancyAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict occupancy adjustment store.</summary>
public static class StrictOccupancyAdjustmentStore
{
    public static IStrictOccupancyAdjustmentStore NewMemoryStrictOccupancyAdjustmentStore()
    {
        return new MemoryStrictOccupancyAdjustmentStore(
            new Dictionary<string, StrictOccupancyAdjustment>(),
            new List<string>());
    }

    public static IStrictOccupancyAdjustmentStore NewCachedStrictOccupancyAdjustmentStore(MemoryStrictOccupancyAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedStrictOccupancyAdjustmentStore(
            inner,
            StrictOccupancyAdjustmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<StrictOccupancyAdjustment> FindStrictOccupancyAdjustments(IStrictOccupancyAdjustmentStore store, StrictOccupancyAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
