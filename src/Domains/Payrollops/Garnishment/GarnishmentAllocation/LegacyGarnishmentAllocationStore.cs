namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentAllocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy garnishment allocations by code.</summary>
public interface ILegacyGarnishmentAllocationStore
{
    LegacyGarnishmentAllocation? Find(string code);
    void Save(LegacyGarnishmentAllocation item);
    bool Remove(string code);
    List<LegacyGarnishmentAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyGarnishmentAllocationStore(Dictionary<string, LegacyGarnishmentAllocation> items, List<string> order) : ILegacyGarnishmentAllocationStore
{
    public Dictionary<string, LegacyGarnishmentAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyGarnishmentAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyGarnishmentAllocation item)
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

    public List<LegacyGarnishmentAllocation> List()
    {
        var outValue = new List<LegacyGarnishmentAllocation>();
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
public sealed class CachedLegacyGarnishmentAllocationStore(MemoryLegacyGarnishmentAllocationStore inner, LegacyGarnishmentAllocationCache cache, int hits, int misses) : ILegacyGarnishmentAllocationStore
{
    public MemoryLegacyGarnishmentAllocationStore Inner { get; set; } = inner;
    public LegacyGarnishmentAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyGarnishmentAllocation? Find(string code)
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

    public void Save(LegacyGarnishmentAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyGarnishmentAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy garnishment allocations. Empty fields match all.</summary>
public sealed record LegacyGarnishmentAllocationQuery(string Label, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyGarnishmentAllocation item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyGarnishmentAllocation> Apply(List<LegacyGarnishmentAllocation> items)
    {
        var matched = Collections.Filter(items, (LegacyGarnishmentAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy garnishment allocation store.</summary>
public static class LegacyGarnishmentAllocationStore
{
    public static ILegacyGarnishmentAllocationStore NewMemoryLegacyGarnishmentAllocationStore()
    {
        return new MemoryLegacyGarnishmentAllocationStore(
            new Dictionary<string, LegacyGarnishmentAllocation>(),
            new List<string>());
    }

    public static ILegacyGarnishmentAllocationStore NewCachedLegacyGarnishmentAllocationStore(MemoryLegacyGarnishmentAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyGarnishmentAllocationStore(
            inner,
            LegacyGarnishmentAllocationCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LegacyGarnishmentAllocation> FindLegacyGarnishmentAllocations(ILegacyGarnishmentAllocationStore store, LegacyGarnishmentAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
