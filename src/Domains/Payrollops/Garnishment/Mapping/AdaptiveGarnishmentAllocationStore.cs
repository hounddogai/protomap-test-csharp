namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive garnishment allocations by code.</summary>
public interface IAdaptiveGarnishmentAllocationStore
{
    AdaptiveGarnishmentAllocation? Find(string code);
    void Save(AdaptiveGarnishmentAllocation item);
    bool Remove(string code);
    List<AdaptiveGarnishmentAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveGarnishmentAllocationStore(Dictionary<string, AdaptiveGarnishmentAllocation> items, List<string> order) : IAdaptiveGarnishmentAllocationStore
{
    public Dictionary<string, AdaptiveGarnishmentAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveGarnishmentAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveGarnishmentAllocation item)
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

    public List<AdaptiveGarnishmentAllocation> List()
    {
        var outValue = new List<AdaptiveGarnishmentAllocation>();
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
public sealed class CachedAdaptiveGarnishmentAllocationStore(MemoryAdaptiveGarnishmentAllocationStore inner, AdaptiveGarnishmentAllocationCache cache, int hits, int misses) : IAdaptiveGarnishmentAllocationStore
{
    public MemoryAdaptiveGarnishmentAllocationStore Inner { get; set; } = inner;
    public AdaptiveGarnishmentAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveGarnishmentAllocation? Find(string code)
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

    public void Save(AdaptiveGarnishmentAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveGarnishmentAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive garnishment allocations. Empty fields match all.</summary>
public sealed record AdaptiveGarnishmentAllocationQuery(string Category, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveGarnishmentAllocation item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveGarnishmentAllocation> Apply(List<AdaptiveGarnishmentAllocation> items)
    {
        var matched = Collections.Filter(items, (AdaptiveGarnishmentAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive garnishment allocation store.</summary>
public static class AdaptiveGarnishmentAllocationStore
{
    public static IAdaptiveGarnishmentAllocationStore NewMemoryAdaptiveGarnishmentAllocationStore()
    {
        return new MemoryAdaptiveGarnishmentAllocationStore(
            new Dictionary<string, AdaptiveGarnishmentAllocation>(),
            new List<string>());
    }

    public static IAdaptiveGarnishmentAllocationStore NewCachedAdaptiveGarnishmentAllocationStore(MemoryAdaptiveGarnishmentAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveGarnishmentAllocationStore(
            inner,
            AdaptiveGarnishmentAllocationCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveGarnishmentAllocation> FindAdaptiveGarnishmentAllocations(IAdaptiveGarnishmentAllocationStore store, AdaptiveGarnishmentAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
