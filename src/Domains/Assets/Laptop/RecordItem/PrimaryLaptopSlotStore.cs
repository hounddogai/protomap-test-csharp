namespace Acme.Workforce.Assets.Laptop.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary laptop slots by code.</summary>
public interface IPrimaryLaptopSlotStore
{
    PrimaryLaptopSlot? Find(string code);
    void Save(PrimaryLaptopSlot item);
    bool Remove(string code);
    List<PrimaryLaptopSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryLaptopSlotStore(Dictionary<string, PrimaryLaptopSlot> items, List<string> order) : IPrimaryLaptopSlotStore
{
    public Dictionary<string, PrimaryLaptopSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryLaptopSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryLaptopSlot item)
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

    public List<PrimaryLaptopSlot> List()
    {
        var outValue = new List<PrimaryLaptopSlot>();
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
public sealed class CachedPrimaryLaptopSlotStore(MemoryPrimaryLaptopSlotStore inner, PrimaryLaptopSlotCache cache, int hits, int misses) : IPrimaryLaptopSlotStore
{
    public MemoryPrimaryLaptopSlotStore Inner { get; set; } = inner;
    public PrimaryLaptopSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryLaptopSlot? Find(string code)
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

    public void Save(PrimaryLaptopSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryLaptopSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary laptop slots. Empty fields match all.</summary>
public sealed record PrimaryLaptopSlotQuery(string Department, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryLaptopSlot item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<PrimaryLaptopSlot> Apply(List<PrimaryLaptopSlot> items)
    {
        var matched = Collections.Filter(items, (PrimaryLaptopSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary laptop slot store.</summary>
public static class PrimaryLaptopSlotStore
{
    public static IPrimaryLaptopSlotStore NewMemoryPrimaryLaptopSlotStore()
    {
        return new MemoryPrimaryLaptopSlotStore(new Dictionary<string, PrimaryLaptopSlot>(), new List<string>());
    }

    public static IPrimaryLaptopSlotStore NewCachedPrimaryLaptopSlotStore(MemoryPrimaryLaptopSlotStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryLaptopSlotStore(inner, PrimaryLaptopSlotCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PrimaryLaptopSlot> FindPrimaryLaptopSlots(IPrimaryLaptopSlotStore store, PrimaryLaptopSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
