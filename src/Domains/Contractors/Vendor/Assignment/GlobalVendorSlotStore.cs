namespace Acme.Workforce.Contractors.Vendor.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global vendor slots by code.</summary>
public interface IGlobalVendorSlotStore
{
    GlobalVendorSlot? Find(string code);
    void Save(GlobalVendorSlot item);
    bool Remove(string code);
    List<GlobalVendorSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalVendorSlotStore(Dictionary<string, GlobalVendorSlot> items, List<string> order) : IGlobalVendorSlotStore
{
    public Dictionary<string, GlobalVendorSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalVendorSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalVendorSlot item)
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

    public List<GlobalVendorSlot> List()
    {
        var outValue = new List<GlobalVendorSlot>();
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
public sealed class CachedGlobalVendorSlotStore(MemoryGlobalVendorSlotStore inner, GlobalVendorSlotCache cache, int hits, int misses) : IGlobalVendorSlotStore
{
    public MemoryGlobalVendorSlotStore Inner { get; set; } = inner;
    public GlobalVendorSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalVendorSlot? Find(string code)
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

    public void Save(GlobalVendorSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalVendorSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global vendor slots. Empty fields match all.</summary>
public sealed record GlobalVendorSlotQuery(string Department, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalVendorSlot item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalVendorSlot> Apply(List<GlobalVendorSlot> items)
    {
        var matched = Collections.Filter(items, (GlobalVendorSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global vendor slot store.</summary>
public static class GlobalVendorSlotStore
{
    public static IGlobalVendorSlotStore NewMemoryGlobalVendorSlotStore()
    {
        return new MemoryGlobalVendorSlotStore(new Dictionary<string, GlobalVendorSlot>(), new List<string>());
    }

    public static IGlobalVendorSlotStore NewCachedGlobalVendorSlotStore(MemoryGlobalVendorSlotStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalVendorSlotStore(inner, GlobalVendorSlotCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<GlobalVendorSlot> FindGlobalVendorSlots(IGlobalVendorSlotStore store, GlobalVendorSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
