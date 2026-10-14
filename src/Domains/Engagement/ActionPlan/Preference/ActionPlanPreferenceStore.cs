namespace Acme.Workforce.Engagement.ActionPlan.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan preferences by code.</summary>
public interface IActionPlanPreferenceStore
{
    ActionPlanPreference? Find(string code);
    void Save(ActionPlanPreference item);
    bool Remove(string code);
    List<ActionPlanPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanPreferenceStore(Dictionary<string, ActionPlanPreference> items, List<string> order) : IActionPlanPreferenceStore
{
    public Dictionary<string, ActionPlanPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanPreference item)
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

    public List<ActionPlanPreference> List()
    {
        var outValue = new List<ActionPlanPreference>();
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
public sealed class CachedActionPlanPreferenceStore(MemoryActionPlanPreferenceStore inner, ActionPlanPreferenceCache cache, int hits, int misses) : IActionPlanPreferenceStore
{
    public MemoryActionPlanPreferenceStore Inner { get; set; } = inner;
    public ActionPlanPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanPreference? Find(string code)
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

    public void Save(ActionPlanPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan preferences. Empty fields match all.</summary>
public sealed record ActionPlanPreferenceQuery(string Name, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanPreference item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<ActionPlanPreference> Apply(List<ActionPlanPreference> items)
    {
        var matched = Collections.Filter(items, (ActionPlanPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan preference store.</summary>
public static class ActionPlanPreferenceStore
{
    public static IActionPlanPreferenceStore NewMemoryActionPlanPreferenceStore()
    {
        return new MemoryActionPlanPreferenceStore(
            new Dictionary<string, ActionPlanPreference>(),
            new List<string>());
    }

    public static IActionPlanPreferenceStore NewCachedActionPlanPreferenceStore(MemoryActionPlanPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanPreferenceStore(inner, ActionPlanPreferenceCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ActionPlanPreference> FindActionPlanPreferences(IActionPlanPreferenceStore store, ActionPlanPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
