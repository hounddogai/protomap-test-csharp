namespace Acme.Workforce.Compensation.BonusPool.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool slots by code.</summary>
public interface IBonusPoolSlotStore
{
    BonusPoolSlot? Find(string code);
    void Save(BonusPoolSlot item);
    bool Remove(string code);
    List<BonusPoolSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolSlotStore(Dictionary<string, BonusPoolSlot> items, List<string> order) : IBonusPoolSlotStore
{
    public Dictionary<string, BonusPoolSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolSlot item)
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

    public List<BonusPoolSlot> List()
    {
        var outValue = new List<BonusPoolSlot>();
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
public sealed class CachedBonusPoolSlotStore(MemoryBonusPoolSlotStore inner, BonusPoolSlotCache cache, int hits, int misses) : IBonusPoolSlotStore
{
    public MemoryBonusPoolSlotStore Inner { get; set; } = inner;
    public BonusPoolSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolSlot? Find(string code)
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

    public void Save(BonusPoolSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool slots. Empty fields match all.</summary>
public sealed record BonusPoolSlotQuery(string Name, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolSlot item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolSlot> Apply(List<BonusPoolSlot> items)
    {
        var matched = Collections.Filter(items, (BonusPoolSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool slot store.</summary>
public static class BonusPoolSlotStore
{
    public static IBonusPoolSlotStore NewMemoryBonusPoolSlotStore()
    {
        return new MemoryBonusPoolSlotStore(new Dictionary<string, BonusPoolSlot>(), new List<string>());
    }

    public static IBonusPoolSlotStore NewCachedBonusPoolSlotStore(MemoryBonusPoolSlotStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolSlotStore(inner, BonusPoolSlotCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BonusPoolSlot> FindBonusPoolSlots(IBonusPoolSlotStore store, BonusPoolSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
