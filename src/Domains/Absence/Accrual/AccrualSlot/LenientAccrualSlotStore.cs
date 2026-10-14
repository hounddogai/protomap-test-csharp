namespace Acme.Workforce.Absence.Accrual.AccrualSlot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient accrual slots by code.</summary>
public interface ILenientAccrualSlotStore
{
    LenientAccrualSlot? Find(string code);
    void Save(LenientAccrualSlot item);
    bool Remove(string code);
    List<LenientAccrualSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientAccrualSlotStore(Dictionary<string, LenientAccrualSlot> items, List<string> order) : ILenientAccrualSlotStore
{
    public Dictionary<string, LenientAccrualSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientAccrualSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientAccrualSlot item)
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

    public List<LenientAccrualSlot> List()
    {
        var outValue = new List<LenientAccrualSlot>();
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
public sealed class CachedLenientAccrualSlotStore(MemoryLenientAccrualSlotStore inner, LenientAccrualSlotCache cache, int hits, int misses) : ILenientAccrualSlotStore
{
    public MemoryLenientAccrualSlotStore Inner { get; set; } = inner;
    public LenientAccrualSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientAccrualSlot? Find(string code)
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

    public void Save(LenientAccrualSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientAccrualSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient accrual slots. Empty fields match all.</summary>
public sealed record LenientAccrualSlotQuery(string Label, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(LenientAccrualSlot item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientAccrualSlot> Apply(List<LenientAccrualSlot> items)
    {
        var matched = Collections.Filter(items, (LenientAccrualSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient accrual slot store.</summary>
public static class LenientAccrualSlotStore
{
    public static ILenientAccrualSlotStore NewMemoryLenientAccrualSlotStore()
    {
        return new MemoryLenientAccrualSlotStore(new Dictionary<string, LenientAccrualSlot>(), new List<string>());
    }

    public static ILenientAccrualSlotStore NewCachedLenientAccrualSlotStore(MemoryLenientAccrualSlotStore inner, int ttl, IClock clock)
    {
        return new CachedLenientAccrualSlotStore(inner, LenientAccrualSlotCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LenientAccrualSlot> FindLenientAccrualSlots(ILenientAccrualSlotStore store, LenientAccrualSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
