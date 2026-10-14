namespace Acme.Workforce.Workplace.Desk.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk slots by code.</summary>
public interface IDeskSlotStore
{
    DeskSlot? Find(string code);
    void Save(DeskSlot item);
    bool Remove(string code);
    List<DeskSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskSlotStore(Dictionary<string, DeskSlot> items, List<string> order) : IDeskSlotStore
{
    public Dictionary<string, DeskSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskSlot item)
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

    public List<DeskSlot> List()
    {
        var outValue = new List<DeskSlot>();
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
public sealed class CachedDeskSlotStore(MemoryDeskSlotStore inner, DeskSlotCache cache, int hits, int misses) : IDeskSlotStore
{
    public MemoryDeskSlotStore Inner { get; set; } = inner;
    public DeskSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskSlot? Find(string code)
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

    public void Save(DeskSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk slots. Empty fields match all.</summary>
public sealed record DeskSlotQuery(string Country, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(DeskSlot item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskSlot> Apply(List<DeskSlot> items)
    {
        var matched = Collections.Filter(items, (DeskSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk slot store.</summary>
public static class DeskSlotStore
{
    public static IDeskSlotStore NewMemoryDeskSlotStore()
    {
        return new MemoryDeskSlotStore(new Dictionary<string, DeskSlot>(), new List<string>());
    }

    public static IDeskSlotStore NewCachedDeskSlotStore(MemoryDeskSlotStore inner, int ttl, IClock clock)
    {
        return new CachedDeskSlotStore(inner, DeskSlotCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DeskSlot> FindDeskSlots(IDeskSlotStore store, DeskSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
