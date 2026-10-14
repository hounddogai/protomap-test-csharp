namespace Acme.Workforce.Engagement.ActionPlan.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan allocations by code.</summary>
public interface IActionPlanAllocationStore
{
    ActionPlanAllocation? Find(string code);
    void Save(ActionPlanAllocation item);
    bool Remove(string code);
    List<ActionPlanAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanAllocationStore(Dictionary<string, ActionPlanAllocation> items, List<string> order) : IActionPlanAllocationStore
{
    public Dictionary<string, ActionPlanAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanAllocation item)
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

    public List<ActionPlanAllocation> List()
    {
        var outValue = new List<ActionPlanAllocation>();
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
public sealed class CachedActionPlanAllocationStore(MemoryActionPlanAllocationStore inner, ActionPlanAllocationCache cache, int hits, int misses) : IActionPlanAllocationStore
{
    public MemoryActionPlanAllocationStore Inner { get; set; } = inner;
    public ActionPlanAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanAllocation? Find(string code)
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

    public void Save(ActionPlanAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan allocations. Empty fields match all.</summary>
public sealed record ActionPlanAllocationQuery(string Channel, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanAllocation item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ActionPlanAllocation> Apply(List<ActionPlanAllocation> items)
    {
        var matched = Collections.Filter(items, (ActionPlanAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan allocation store.</summary>
public static class ActionPlanAllocationStore
{
    public static IActionPlanAllocationStore NewMemoryActionPlanAllocationStore()
    {
        return new MemoryActionPlanAllocationStore(
            new Dictionary<string, ActionPlanAllocation>(),
            new List<string>());
    }

    public static IActionPlanAllocationStore NewCachedActionPlanAllocationStore(MemoryActionPlanAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanAllocationStore(inner, ActionPlanAllocationCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ActionPlanAllocation> FindActionPlanAllocations(IActionPlanAllocationStore store, ActionPlanAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
