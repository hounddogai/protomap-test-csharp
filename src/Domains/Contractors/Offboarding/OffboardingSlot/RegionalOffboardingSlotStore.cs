namespace Acme.Workforce.Contractors.Offboarding.OffboardingSlot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional offboarding slots by code.</summary>
public interface IRegionalOffboardingSlotStore
{
    RegionalOffboardingSlot? Find(string code);
    void Save(RegionalOffboardingSlot item);
    bool Remove(string code);
    List<RegionalOffboardingSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalOffboardingSlotStore(Dictionary<string, RegionalOffboardingSlot> items, List<string> order) : IRegionalOffboardingSlotStore
{
    public Dictionary<string, RegionalOffboardingSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalOffboardingSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalOffboardingSlot item)
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

    public List<RegionalOffboardingSlot> List()
    {
        var outValue = new List<RegionalOffboardingSlot>();
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
public sealed class CachedRegionalOffboardingSlotStore(MemoryRegionalOffboardingSlotStore inner, RegionalOffboardingSlotCache cache, int hits, int misses) : IRegionalOffboardingSlotStore
{
    public MemoryRegionalOffboardingSlotStore Inner { get; set; } = inner;
    public RegionalOffboardingSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalOffboardingSlot? Find(string code)
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

    public void Save(RegionalOffboardingSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalOffboardingSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional offboarding slots. Empty fields match all.</summary>
public sealed record RegionalOffboardingSlotQuery(string Department, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalOffboardingSlot item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalOffboardingSlot> Apply(List<RegionalOffboardingSlot> items)
    {
        var matched = Collections.Filter(items, (RegionalOffboardingSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional offboarding slot store.</summary>
public static class RegionalOffboardingSlotStore
{
    public static IRegionalOffboardingSlotStore NewMemoryRegionalOffboardingSlotStore()
    {
        return new MemoryRegionalOffboardingSlotStore(
            new Dictionary<string, RegionalOffboardingSlot>(),
            new List<string>());
    }

    public static IRegionalOffboardingSlotStore NewCachedRegionalOffboardingSlotStore(MemoryRegionalOffboardingSlotStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalOffboardingSlotStore(
            inner,
            RegionalOffboardingSlotCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RegionalOffboardingSlot> FindRegionalOffboardingSlots(IRegionalOffboardingSlotStore store, RegionalOffboardingSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
