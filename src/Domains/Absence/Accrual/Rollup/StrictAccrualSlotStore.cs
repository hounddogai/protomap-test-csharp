namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict accrual slots by code.</summary>
public interface IStrictAccrualSlotStore
{
    StrictAccrualSlot? Find(string code);
    void Save(StrictAccrualSlot item);
    bool Remove(string code);
    List<StrictAccrualSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictAccrualSlotStore(Dictionary<string, StrictAccrualSlot> items, List<string> order) : IStrictAccrualSlotStore
{
    public Dictionary<string, StrictAccrualSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictAccrualSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictAccrualSlot item)
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

    public List<StrictAccrualSlot> List()
    {
        var outValue = new List<StrictAccrualSlot>();
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
public sealed class CachedStrictAccrualSlotStore(MemoryStrictAccrualSlotStore inner, StrictAccrualSlotCache cache, int hits, int misses) : IStrictAccrualSlotStore
{
    public MemoryStrictAccrualSlotStore Inner { get; set; } = inner;
    public StrictAccrualSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictAccrualSlot? Find(string code)
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

    public void Save(StrictAccrualSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictAccrualSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict accrual slots. Empty fields match all.</summary>
public sealed record StrictAccrualSlotQuery(string Department, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(StrictAccrualSlot item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictAccrualSlot> Apply(List<StrictAccrualSlot> items)
    {
        var matched = Collections.Filter(items, (StrictAccrualSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict accrual slot store.</summary>
public static class StrictAccrualSlotStore
{
    public static IStrictAccrualSlotStore NewMemoryStrictAccrualSlotStore()
    {
        return new MemoryStrictAccrualSlotStore(new Dictionary<string, StrictAccrualSlot>(), new List<string>());
    }

    public static IStrictAccrualSlotStore NewCachedStrictAccrualSlotStore(MemoryStrictAccrualSlotStore inner, int ttl, IClock clock)
    {
        return new CachedStrictAccrualSlotStore(inner, StrictAccrualSlotCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<StrictAccrualSlot> FindStrictAccrualSlots(IStrictAccrualSlotStore store, StrictAccrualSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
