namespace Acme.Workforce.Assets.Laptop.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global laptop slots by code.</summary>
public interface IGlobalLaptopSlotStore
{
    GlobalLaptopSlot? Find(string code);
    void Save(GlobalLaptopSlot item);
    bool Remove(string code);
    List<GlobalLaptopSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalLaptopSlotStore(Dictionary<string, GlobalLaptopSlot> items, List<string> order) : IGlobalLaptopSlotStore
{
    public Dictionary<string, GlobalLaptopSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalLaptopSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalLaptopSlot item)
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

    public List<GlobalLaptopSlot> List()
    {
        var outValue = new List<GlobalLaptopSlot>();
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
public sealed class CachedGlobalLaptopSlotStore(MemoryGlobalLaptopSlotStore inner, GlobalLaptopSlotCache cache, int hits, int misses) : IGlobalLaptopSlotStore
{
    public MemoryGlobalLaptopSlotStore Inner { get; set; } = inner;
    public GlobalLaptopSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalLaptopSlot? Find(string code)
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

    public void Save(GlobalLaptopSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalLaptopSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global laptop slots. Empty fields match all.</summary>
public sealed record GlobalLaptopSlotQuery(string Name, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalLaptopSlot item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalLaptopSlot> Apply(List<GlobalLaptopSlot> items)
    {
        var matched = Collections.Filter(items, (GlobalLaptopSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global laptop slot store.</summary>
public static class GlobalLaptopSlotStore
{
    public static IGlobalLaptopSlotStore NewMemoryGlobalLaptopSlotStore()
    {
        return new MemoryGlobalLaptopSlotStore(new Dictionary<string, GlobalLaptopSlot>(), new List<string>());
    }

    public static IGlobalLaptopSlotStore NewCachedGlobalLaptopSlotStore(MemoryGlobalLaptopSlotStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalLaptopSlotStore(inner, GlobalLaptopSlotCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<GlobalLaptopSlot> FindGlobalLaptopSlots(IGlobalLaptopSlotStore store, GlobalLaptopSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
