namespace Acme.Workforce.Payrollops.Deduction.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction slots by code.</summary>
public interface IDeductionSlotStore
{
    DeductionSlot? Find(string code);
    void Save(DeductionSlot item);
    bool Remove(string code);
    List<DeductionSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionSlotStore(Dictionary<string, DeductionSlot> items, List<string> order) : IDeductionSlotStore
{
    public Dictionary<string, DeductionSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionSlot item)
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

    public List<DeductionSlot> List()
    {
        var outValue = new List<DeductionSlot>();
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
public sealed class CachedDeductionSlotStore(MemoryDeductionSlotStore inner, DeductionSlotCache cache, int hits, int misses) : IDeductionSlotStore
{
    public MemoryDeductionSlotStore Inner { get; set; } = inner;
    public DeductionSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionSlot? Find(string code)
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

    public void Save(DeductionSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction slots. Empty fields match all.</summary>
public sealed record DeductionSlotQuery(string Unit, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionSlot item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeductionSlot> Apply(List<DeductionSlot> items)
    {
        var matched = Collections.Filter(items, (DeductionSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction slot store.</summary>
public static class DeductionSlotStore
{
    public static IDeductionSlotStore NewMemoryDeductionSlotStore()
    {
        return new MemoryDeductionSlotStore(new Dictionary<string, DeductionSlot>(), new List<string>());
    }

    public static IDeductionSlotStore NewCachedDeductionSlotStore(MemoryDeductionSlotStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionSlotStore(inner, DeductionSlotCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<DeductionSlot> FindDeductionSlots(IDeductionSlotStore store, DeductionSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
