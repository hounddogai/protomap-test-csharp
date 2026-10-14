namespace Acme.Workforce.Talent.SuccessionPlan.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional succession plan allocations by code.</summary>
public interface IRegionalSuccessionPlanAllocationStore
{
    RegionalSuccessionPlanAllocation? Find(string code);
    void Save(RegionalSuccessionPlanAllocation item);
    bool Remove(string code);
    List<RegionalSuccessionPlanAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalSuccessionPlanAllocationStore(Dictionary<string, RegionalSuccessionPlanAllocation> items, List<string> order) : IRegionalSuccessionPlanAllocationStore
{
    public Dictionary<string, RegionalSuccessionPlanAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalSuccessionPlanAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalSuccessionPlanAllocation item)
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

    public List<RegionalSuccessionPlanAllocation> List()
    {
        var outValue = new List<RegionalSuccessionPlanAllocation>();
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
public sealed class CachedRegionalSuccessionPlanAllocationStore(MemoryRegionalSuccessionPlanAllocationStore inner, RegionalSuccessionPlanAllocationCache cache, int hits, int misses) : IRegionalSuccessionPlanAllocationStore
{
    public MemoryRegionalSuccessionPlanAllocationStore Inner { get; set; } = inner;
    public RegionalSuccessionPlanAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalSuccessionPlanAllocation? Find(string code)
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

    public void Save(RegionalSuccessionPlanAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalSuccessionPlanAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional succession plan allocations. Empty fields match all.</summary>
public sealed record RegionalSuccessionPlanAllocationQuery(string Department, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalSuccessionPlanAllocation item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalSuccessionPlanAllocation> Apply(List<RegionalSuccessionPlanAllocation> items)
    {
        var matched = Collections.Filter(items, (RegionalSuccessionPlanAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional succession plan allocation store.</summary>
public static class RegionalSuccessionPlanAllocationStore
{
    public static IRegionalSuccessionPlanAllocationStore NewMemoryRegionalSuccessionPlanAllocationStore()
    {
        return new MemoryRegionalSuccessionPlanAllocationStore(
            new Dictionary<string, RegionalSuccessionPlanAllocation>(),
            new List<string>());
    }

    public static IRegionalSuccessionPlanAllocationStore NewCachedRegionalSuccessionPlanAllocationStore(MemoryRegionalSuccessionPlanAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalSuccessionPlanAllocationStore(
            inner,
            RegionalSuccessionPlanAllocationCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RegionalSuccessionPlanAllocation> FindRegionalSuccessionPlanAllocations(IRegionalSuccessionPlanAllocationStore store, RegionalSuccessionPlanAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
