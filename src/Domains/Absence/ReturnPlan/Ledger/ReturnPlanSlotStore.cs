namespace Acme.Workforce.Absence.ReturnPlan.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan slots by code.</summary>
public interface IReturnPlanSlotStore
{
    ReturnPlanSlot? Find(string code);
    void Save(ReturnPlanSlot item);
    bool Remove(string code);
    List<ReturnPlanSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanSlotStore(Dictionary<string, ReturnPlanSlot> items, List<string> order) : IReturnPlanSlotStore
{
    public Dictionary<string, ReturnPlanSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanSlot item)
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

    public List<ReturnPlanSlot> List()
    {
        var outValue = new List<ReturnPlanSlot>();
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
public sealed class CachedReturnPlanSlotStore(MemoryReturnPlanSlotStore inner, ReturnPlanSlotCache cache, int hits, int misses) : IReturnPlanSlotStore
{
    public MemoryReturnPlanSlotStore Inner { get; set; } = inner;
    public ReturnPlanSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanSlot? Find(string code)
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

    public void Save(ReturnPlanSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan slots. Empty fields match all.</summary>
public sealed record ReturnPlanSlotQuery(string Unit, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanSlot item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<ReturnPlanSlot> Apply(List<ReturnPlanSlot> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan slot store.</summary>
public static class ReturnPlanSlotStore
{
    public static IReturnPlanSlotStore NewMemoryReturnPlanSlotStore()
    {
        return new MemoryReturnPlanSlotStore(new Dictionary<string, ReturnPlanSlot>(), new List<string>());
    }

    public static IReturnPlanSlotStore NewCachedReturnPlanSlotStore(MemoryReturnPlanSlotStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanSlotStore(inner, ReturnPlanSlotCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanSlot> FindReturnPlanSlots(IReturnPlanSlotStore store, ReturnPlanSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
