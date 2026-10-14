namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps license seat allocations by code.</summary>
public interface ILicenseSeatAllocationStore
{
    LicenseSeatAllocation? Find(string code);
    void Save(LicenseSeatAllocation item);
    bool Remove(string code);
    List<LicenseSeatAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLicenseSeatAllocationStore(Dictionary<string, LicenseSeatAllocation> items, List<string> order) : ILicenseSeatAllocationStore
{
    public Dictionary<string, LicenseSeatAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LicenseSeatAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LicenseSeatAllocation item)
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

    public List<LicenseSeatAllocation> List()
    {
        var outValue = new List<LicenseSeatAllocation>();
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
public sealed class CachedLicenseSeatAllocationStore(MemoryLicenseSeatAllocationStore inner, LicenseSeatAllocationCache cache, int hits, int misses) : ILicenseSeatAllocationStore
{
    public MemoryLicenseSeatAllocationStore Inner { get; set; } = inner;
    public LicenseSeatAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LicenseSeatAllocation? Find(string code)
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

    public void Save(LicenseSeatAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LicenseSeatAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects license seat allocations. Empty fields match all.</summary>
public sealed record LicenseSeatAllocationQuery(string Source, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(LicenseSeatAllocation item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LicenseSeatAllocation> Apply(List<LicenseSeatAllocation> items)
    {
        var matched = Collections.Filter(items, (LicenseSeatAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of license seat allocation store.</summary>
public static class LicenseSeatAllocationStore
{
    public static ILicenseSeatAllocationStore NewMemoryLicenseSeatAllocationStore()
    {
        return new MemoryLicenseSeatAllocationStore(
            new Dictionary<string, LicenseSeatAllocation>(),
            new List<string>());
    }

    public static ILicenseSeatAllocationStore NewCachedLicenseSeatAllocationStore(MemoryLicenseSeatAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedLicenseSeatAllocationStore(
            inner,
            LicenseSeatAllocationCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LicenseSeatAllocation> FindLicenseSeatAllocations(ILicenseSeatAllocationStore store, LicenseSeatAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
