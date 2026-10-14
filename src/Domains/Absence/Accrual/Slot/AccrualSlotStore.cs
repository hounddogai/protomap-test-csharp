namespace Acme.Workforce.Absence.Accrual.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual slots by code.</summary>
public interface IAccrualSlotStore
{
    AccrualSlot? Find(string code);
    void Save(AccrualSlot item);
    bool Remove(string code);
    List<AccrualSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualSlotStore(Dictionary<string, AccrualSlot> items, List<string> order) : IAccrualSlotStore
{
    public Dictionary<string, AccrualSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualSlot item)
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

    public List<AccrualSlot> List()
    {
        var outValue = new List<AccrualSlot>();
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
public sealed class CachedAccrualSlotStore(MemoryAccrualSlotStore inner, AccrualSlotCache cache, int hits, int misses) : IAccrualSlotStore
{
    public MemoryAccrualSlotStore Inner { get; set; } = inner;
    public AccrualSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualSlot? Find(string code)
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

    public void Save(AccrualSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual slots. Empty fields match all.</summary>
public sealed record AccrualSlotQuery(string Locale, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualSlot item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccrualSlot> Apply(List<AccrualSlot> items)
    {
        var matched = Collections.Filter(items, (AccrualSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual slot store.</summary>
public static class AccrualSlotStore
{
    public static IAccrualSlotStore NewMemoryAccrualSlotStore()
    {
        return new MemoryAccrualSlotStore(new Dictionary<string, AccrualSlot>(), new List<string>());
    }

    public static IAccrualSlotStore NewCachedAccrualSlotStore(MemoryAccrualSlotStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualSlotStore(inner, AccrualSlotCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AccrualSlot> FindAccrualSlots(IAccrualSlotStore store, AccrualSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
