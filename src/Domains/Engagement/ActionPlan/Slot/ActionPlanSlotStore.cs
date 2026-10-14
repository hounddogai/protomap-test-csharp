namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan slots by code.</summary>
public interface IActionPlanSlotStore
{
    ActionPlanSlot? Find(string code);
    void Save(ActionPlanSlot item);
    bool Remove(string code);
    List<ActionPlanSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanSlotStore(Dictionary<string, ActionPlanSlot> items, List<string> order) : IActionPlanSlotStore
{
    public Dictionary<string, ActionPlanSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanSlot item)
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

    public List<ActionPlanSlot> List()
    {
        var outValue = new List<ActionPlanSlot>();
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
public sealed class CachedActionPlanSlotStore(MemoryActionPlanSlotStore inner, ActionPlanSlotCache cache, int hits, int misses) : IActionPlanSlotStore
{
    public MemoryActionPlanSlotStore Inner { get; set; } = inner;
    public ActionPlanSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanSlot? Find(string code)
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

    public void Save(ActionPlanSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan slots. Empty fields match all.</summary>
public sealed record ActionPlanSlotQuery(string Name, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanSlot item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ActionPlanSlot> Apply(List<ActionPlanSlot> items)
    {
        var matched = Collections.Filter(items, (ActionPlanSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan slot store.</summary>
public static class ActionPlanSlotStore
{
    public static IActionPlanSlotStore NewMemoryActionPlanSlotStore()
    {
        return new MemoryActionPlanSlotStore(new Dictionary<string, ActionPlanSlot>(), new List<string>());
    }

    public static IActionPlanSlotStore NewCachedActionPlanSlotStore(MemoryActionPlanSlotStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanSlotStore(inner, ActionPlanSlotCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ActionPlanSlot> FindActionPlanSlots(IActionPlanSlotStore store, ActionPlanSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
