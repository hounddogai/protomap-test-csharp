namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary action plan slots by code.</summary>
public interface IPrimaryActionPlanSlotStore
{
    PrimaryActionPlanSlot? Find(string code);
    void Save(PrimaryActionPlanSlot item);
    bool Remove(string code);
    List<PrimaryActionPlanSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryActionPlanSlotStore(Dictionary<string, PrimaryActionPlanSlot> items, List<string> order) : IPrimaryActionPlanSlotStore
{
    public Dictionary<string, PrimaryActionPlanSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryActionPlanSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryActionPlanSlot item)
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

    public List<PrimaryActionPlanSlot> List()
    {
        var outValue = new List<PrimaryActionPlanSlot>();
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
public sealed class CachedPrimaryActionPlanSlotStore(MemoryPrimaryActionPlanSlotStore inner, PrimaryActionPlanSlotCache cache, int hits, int misses) : IPrimaryActionPlanSlotStore
{
    public MemoryPrimaryActionPlanSlotStore Inner { get; set; } = inner;
    public PrimaryActionPlanSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryActionPlanSlot? Find(string code)
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

    public void Save(PrimaryActionPlanSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryActionPlanSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary action plan slots. Empty fields match all.</summary>
public sealed record PrimaryActionPlanSlotQuery(string Locale, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryActionPlanSlot item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<PrimaryActionPlanSlot> Apply(List<PrimaryActionPlanSlot> items)
    {
        var matched = Collections.Filter(items, (PrimaryActionPlanSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary action plan slot store.</summary>
public static class PrimaryActionPlanSlotStore
{
    public static IPrimaryActionPlanSlotStore NewMemoryPrimaryActionPlanSlotStore()
    {
        return new MemoryPrimaryActionPlanSlotStore(
            new Dictionary<string, PrimaryActionPlanSlot>(),
            new List<string>());
    }

    public static IPrimaryActionPlanSlotStore NewCachedPrimaryActionPlanSlotStore(MemoryPrimaryActionPlanSlotStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryActionPlanSlotStore(inner, PrimaryActionPlanSlotCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PrimaryActionPlanSlot> FindPrimaryActionPlanSlots(IPrimaryActionPlanSlotStore store, PrimaryActionPlanSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
