namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy allocations by code.</summary>
public interface IOccupancyAllocationStore
{
    OccupancyAllocation? Find(string code);
    void Save(OccupancyAllocation item);
    bool Remove(string code);
    List<OccupancyAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyAllocationStore(Dictionary<string, OccupancyAllocation> items, List<string> order) : IOccupancyAllocationStore
{
    public Dictionary<string, OccupancyAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyAllocation item)
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

    public List<OccupancyAllocation> List()
    {
        var outValue = new List<OccupancyAllocation>();
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
public sealed class CachedOccupancyAllocationStore(MemoryOccupancyAllocationStore inner, OccupancyAllocationCache cache, int hits, int misses) : IOccupancyAllocationStore
{
    public MemoryOccupancyAllocationStore Inner { get; set; } = inner;
    public OccupancyAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyAllocation? Find(string code)
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

    public void Save(OccupancyAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy allocations. Empty fields match all.</summary>
public sealed record OccupancyAllocationQuery(string Country, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyAllocation item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OccupancyAllocation> Apply(List<OccupancyAllocation> items)
    {
        var matched = Collections.Filter(items, (OccupancyAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy allocation store.</summary>
public static class OccupancyAllocationStore
{
    public static IOccupancyAllocationStore NewMemoryOccupancyAllocationStore()
    {
        return new MemoryOccupancyAllocationStore(new Dictionary<string, OccupancyAllocation>(), new List<string>());
    }

    public static IOccupancyAllocationStore NewCachedOccupancyAllocationStore(MemoryOccupancyAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyAllocationStore(inner, OccupancyAllocationCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<OccupancyAllocation> FindOccupancyAllocations(IOccupancyAllocationStore store, OccupancyAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
