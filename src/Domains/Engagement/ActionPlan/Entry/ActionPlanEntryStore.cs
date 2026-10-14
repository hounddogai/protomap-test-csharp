namespace Acme.Workforce.Engagement.ActionPlan.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan entries by code.</summary>
public interface IActionPlanEntryStore
{
    ActionPlanEntry? Find(string code);
    void Save(ActionPlanEntry item);
    bool Remove(string code);
    List<ActionPlanEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanEntryStore(Dictionary<string, ActionPlanEntry> items, List<string> order) : IActionPlanEntryStore
{
    public Dictionary<string, ActionPlanEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanEntry item)
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

    public List<ActionPlanEntry> List()
    {
        var outValue = new List<ActionPlanEntry>();
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
public sealed class CachedActionPlanEntryStore(MemoryActionPlanEntryStore inner, ActionPlanEntryCache cache, int hits, int misses) : IActionPlanEntryStore
{
    public MemoryActionPlanEntryStore Inner { get; set; } = inner;
    public ActionPlanEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanEntry? Find(string code)
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

    public void Save(ActionPlanEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan entries. Empty fields match all.</summary>
public sealed record ActionPlanEntryQuery(string Segment, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanEntry item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ActionPlanEntry> Apply(List<ActionPlanEntry> items)
    {
        var matched = Collections.Filter(items, (ActionPlanEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan entry store.</summary>
public static class ActionPlanEntryStore
{
    public static IActionPlanEntryStore NewMemoryActionPlanEntryStore()
    {
        return new MemoryActionPlanEntryStore(new Dictionary<string, ActionPlanEntry>(), new List<string>());
    }

    public static IActionPlanEntryStore NewCachedActionPlanEntryStore(MemoryActionPlanEntryStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanEntryStore(inner, ActionPlanEntryCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ActionPlanEntry> FindActionPlanEntries(IActionPlanEntryStore store, ActionPlanEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
