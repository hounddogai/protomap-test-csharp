namespace Acme.Workforce.Engagement.ActionPlan.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan mappings by code.</summary>
public interface IActionPlanMappingStore
{
    ActionPlanMapping? Find(string code);
    void Save(ActionPlanMapping item);
    bool Remove(string code);
    List<ActionPlanMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanMappingStore(Dictionary<string, ActionPlanMapping> items, List<string> order) : IActionPlanMappingStore
{
    public Dictionary<string, ActionPlanMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanMapping item)
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

    public List<ActionPlanMapping> List()
    {
        var outValue = new List<ActionPlanMapping>();
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
public sealed class CachedActionPlanMappingStore(MemoryActionPlanMappingStore inner, ActionPlanMappingCache cache, int hits, int misses) : IActionPlanMappingStore
{
    public MemoryActionPlanMappingStore Inner { get; set; } = inner;
    public ActionPlanMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanMapping? Find(string code)
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

    public void Save(ActionPlanMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan mappings. Empty fields match all.</summary>
public sealed record ActionPlanMappingQuery(string Locale, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanMapping item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<ActionPlanMapping> Apply(List<ActionPlanMapping> items)
    {
        var matched = Collections.Filter(items, (ActionPlanMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan mapping store.</summary>
public static class ActionPlanMappingStore
{
    public static IActionPlanMappingStore NewMemoryActionPlanMappingStore()
    {
        return new MemoryActionPlanMappingStore(new Dictionary<string, ActionPlanMapping>(), new List<string>());
    }

    public static IActionPlanMappingStore NewCachedActionPlanMappingStore(MemoryActionPlanMappingStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanMappingStore(inner, ActionPlanMappingCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ActionPlanMapping> FindActionPlanMappings(IActionPlanMappingStore store, ActionPlanMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
