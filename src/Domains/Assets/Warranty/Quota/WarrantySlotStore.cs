namespace Acme.Workforce.Assets.Warranty.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty slots by code.</summary>
public interface IWarrantySlotStore
{
    WarrantySlot? Find(string code);
    void Save(WarrantySlot item);
    bool Remove(string code);
    List<WarrantySlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantySlotStore(Dictionary<string, WarrantySlot> items, List<string> order) : IWarrantySlotStore
{
    public Dictionary<string, WarrantySlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantySlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantySlot item)
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

    public List<WarrantySlot> List()
    {
        var outValue = new List<WarrantySlot>();
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
public sealed class CachedWarrantySlotStore(MemoryWarrantySlotStore inner, WarrantySlotCache cache, int hits, int misses) : IWarrantySlotStore
{
    public MemoryWarrantySlotStore Inner { get; set; } = inner;
    public WarrantySlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantySlot? Find(string code)
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

    public void Save(WarrantySlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantySlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty slots. Empty fields match all.</summary>
public sealed record WarrantySlotQuery(string Label, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantySlot item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantySlot> Apply(List<WarrantySlot> items)
    {
        var matched = Collections.Filter(items, (WarrantySlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty slot store.</summary>
public static class WarrantySlotStore
{
    public static IWarrantySlotStore NewMemoryWarrantySlotStore()
    {
        return new MemoryWarrantySlotStore(new Dictionary<string, WarrantySlot>(), new List<string>());
    }

    public static IWarrantySlotStore NewCachedWarrantySlotStore(MemoryWarrantySlotStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantySlotStore(inner, WarrantySlotCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<WarrantySlot> FindWarrantySlots(IWarrantySlotStore store, WarrantySlotQuery query)
    {
        return query.Apply(store.List());
    }
}
