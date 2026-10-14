namespace Acme.Workforce.Absence.ReturnPlan.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict return plan slots by code.</summary>
public interface IStrictReturnPlanSlotStore
{
    StrictReturnPlanSlot? Find(string code);
    void Save(StrictReturnPlanSlot item);
    bool Remove(string code);
    List<StrictReturnPlanSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictReturnPlanSlotStore(Dictionary<string, StrictReturnPlanSlot> items, List<string> order) : IStrictReturnPlanSlotStore
{
    public Dictionary<string, StrictReturnPlanSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictReturnPlanSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictReturnPlanSlot item)
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

    public List<StrictReturnPlanSlot> List()
    {
        var outValue = new List<StrictReturnPlanSlot>();
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
public sealed class CachedStrictReturnPlanSlotStore(MemoryStrictReturnPlanSlotStore inner, StrictReturnPlanSlotCache cache, int hits, int misses) : IStrictReturnPlanSlotStore
{
    public MemoryStrictReturnPlanSlotStore Inner { get; set; } = inner;
    public StrictReturnPlanSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictReturnPlanSlot? Find(string code)
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

    public void Save(StrictReturnPlanSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictReturnPlanSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict return plan slots. Empty fields match all.</summary>
public sealed record StrictReturnPlanSlotQuery(string Country, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(StrictReturnPlanSlot item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictReturnPlanSlot> Apply(List<StrictReturnPlanSlot> items)
    {
        var matched = Collections.Filter(items, (StrictReturnPlanSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict return plan slot store.</summary>
public static class StrictReturnPlanSlotStore
{
    public static IStrictReturnPlanSlotStore NewMemoryStrictReturnPlanSlotStore()
    {
        return new MemoryStrictReturnPlanSlotStore(
            new Dictionary<string, StrictReturnPlanSlot>(),
            new List<string>());
    }

    public static IStrictReturnPlanSlotStore NewCachedStrictReturnPlanSlotStore(MemoryStrictReturnPlanSlotStore inner, int ttl, IClock clock)
    {
        return new CachedStrictReturnPlanSlotStore(inner, StrictReturnPlanSlotCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<StrictReturnPlanSlot> FindStrictReturnPlanSlots(IStrictReturnPlanSlotStore store, StrictReturnPlanSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
