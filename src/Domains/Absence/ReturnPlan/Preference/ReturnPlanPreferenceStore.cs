namespace Acme.Workforce.Absence.ReturnPlan.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan preferences by code.</summary>
public interface IReturnPlanPreferenceStore
{
    ReturnPlanPreference? Find(string code);
    void Save(ReturnPlanPreference item);
    bool Remove(string code);
    List<ReturnPlanPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanPreferenceStore(Dictionary<string, ReturnPlanPreference> items, List<string> order) : IReturnPlanPreferenceStore
{
    public Dictionary<string, ReturnPlanPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanPreference item)
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

    public List<ReturnPlanPreference> List()
    {
        var outValue = new List<ReturnPlanPreference>();
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
public sealed class CachedReturnPlanPreferenceStore(MemoryReturnPlanPreferenceStore inner, ReturnPlanPreferenceCache cache, int hits, int misses) : IReturnPlanPreferenceStore
{
    public MemoryReturnPlanPreferenceStore Inner { get; set; } = inner;
    public ReturnPlanPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanPreference? Find(string code)
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

    public void Save(ReturnPlanPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan preferences. Empty fields match all.</summary>
public sealed record ReturnPlanPreferenceQuery(string Department, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanPreference item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<ReturnPlanPreference> Apply(List<ReturnPlanPreference> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan preference store.</summary>
public static class ReturnPlanPreferenceStore
{
    public static IReturnPlanPreferenceStore NewMemoryReturnPlanPreferenceStore()
    {
        return new MemoryReturnPlanPreferenceStore(
            new Dictionary<string, ReturnPlanPreference>(),
            new List<string>());
    }

    public static IReturnPlanPreferenceStore NewCachedReturnPlanPreferenceStore(MemoryReturnPlanPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanPreferenceStore(inner, ReturnPlanPreferenceCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanPreference> FindReturnPlanPreferences(IReturnPlanPreferenceStore store, ReturnPlanPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
