namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment allocations by code.</summary>
public interface IGarnishmentAllocationStore
{
    GarnishmentAllocation? Find(string code);
    void Save(GarnishmentAllocation item);
    bool Remove(string code);
    List<GarnishmentAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentAllocationStore(Dictionary<string, GarnishmentAllocation> items, List<string> order) : IGarnishmentAllocationStore
{
    public Dictionary<string, GarnishmentAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentAllocation item)
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

    public List<GarnishmentAllocation> List()
    {
        var outValue = new List<GarnishmentAllocation>();
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
public sealed class CachedGarnishmentAllocationStore(MemoryGarnishmentAllocationStore inner, GarnishmentAllocationCache cache, int hits, int misses) : IGarnishmentAllocationStore
{
    public MemoryGarnishmentAllocationStore Inner { get; set; } = inner;
    public GarnishmentAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentAllocation? Find(string code)
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

    public void Save(GarnishmentAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment allocations. Empty fields match all.</summary>
public sealed record GarnishmentAllocationQuery(string Region, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentAllocation item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GarnishmentAllocation> Apply(List<GarnishmentAllocation> items)
    {
        var matched = Collections.Filter(items, (GarnishmentAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment allocation store.</summary>
public static class GarnishmentAllocationStore
{
    public static IGarnishmentAllocationStore NewMemoryGarnishmentAllocationStore()
    {
        return new MemoryGarnishmentAllocationStore(
            new Dictionary<string, GarnishmentAllocation>(),
            new List<string>());
    }

    public static IGarnishmentAllocationStore NewCachedGarnishmentAllocationStore(MemoryGarnishmentAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentAllocationStore(inner, GarnishmentAllocationCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<GarnishmentAllocation> FindGarnishmentAllocations(IGarnishmentAllocationStore store, GarnishmentAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
