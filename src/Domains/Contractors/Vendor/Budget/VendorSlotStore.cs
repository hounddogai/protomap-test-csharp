namespace Acme.Workforce.Contractors.Vendor.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor slots by code.</summary>
public interface IVendorSlotStore
{
    VendorSlot? Find(string code);
    void Save(VendorSlot item);
    bool Remove(string code);
    List<VendorSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorSlotStore(Dictionary<string, VendorSlot> items, List<string> order) : IVendorSlotStore
{
    public Dictionary<string, VendorSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorSlot item)
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

    public List<VendorSlot> List()
    {
        var outValue = new List<VendorSlot>();
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
public sealed class CachedVendorSlotStore(MemoryVendorSlotStore inner, VendorSlotCache cache, int hits, int misses) : IVendorSlotStore
{
    public MemoryVendorSlotStore Inner { get; set; } = inner;
    public VendorSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorSlot? Find(string code)
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

    public void Save(VendorSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor slots. Empty fields match all.</summary>
public sealed record VendorSlotQuery(string Locale, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(VendorSlot item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VendorSlot> Apply(List<VendorSlot> items)
    {
        var matched = Collections.Filter(items, (VendorSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor slot store.</summary>
public static class VendorSlotStore
{
    public static IVendorSlotStore NewMemoryVendorSlotStore()
    {
        return new MemoryVendorSlotStore(new Dictionary<string, VendorSlot>(), new List<string>());
    }

    public static IVendorSlotStore NewCachedVendorSlotStore(MemoryVendorSlotStore inner, int ttl, IClock clock)
    {
        return new CachedVendorSlotStore(inner, VendorSlotCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<VendorSlot> FindVendorSlots(IVendorSlotStore store, VendorSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
