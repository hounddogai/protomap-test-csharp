namespace Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual action plan preferences by code.</summary>
public interface IManualActionPlanPreferenceStore
{
    ManualActionPlanPreference? Find(string code);
    void Save(ManualActionPlanPreference item);
    bool Remove(string code);
    List<ManualActionPlanPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualActionPlanPreferenceStore(Dictionary<string, ManualActionPlanPreference> items, List<string> order) : IManualActionPlanPreferenceStore
{
    public Dictionary<string, ManualActionPlanPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualActionPlanPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualActionPlanPreference item)
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

    public List<ManualActionPlanPreference> List()
    {
        var outValue = new List<ManualActionPlanPreference>();
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
public sealed class CachedManualActionPlanPreferenceStore(MemoryManualActionPlanPreferenceStore inner, ManualActionPlanPreferenceCache cache, int hits, int misses) : IManualActionPlanPreferenceStore
{
    public MemoryManualActionPlanPreferenceStore Inner { get; set; } = inner;
    public ManualActionPlanPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualActionPlanPreference? Find(string code)
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

    public void Save(ManualActionPlanPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualActionPlanPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual action plan preferences. Empty fields match all.</summary>
public sealed record ManualActionPlanPreferenceQuery(string Department, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(ManualActionPlanPreference item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualActionPlanPreference> Apply(List<ManualActionPlanPreference> items)
    {
        var matched = Collections.Filter(items, (ManualActionPlanPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual action plan preference store.</summary>
public static class ManualActionPlanPreferenceStore
{
    public static IManualActionPlanPreferenceStore NewMemoryManualActionPlanPreferenceStore()
    {
        return new MemoryManualActionPlanPreferenceStore(
            new Dictionary<string, ManualActionPlanPreference>(),
            new List<string>());
    }

    public static IManualActionPlanPreferenceStore NewCachedManualActionPlanPreferenceStore(MemoryManualActionPlanPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedManualActionPlanPreferenceStore(
            inner,
            ManualActionPlanPreferenceCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ManualActionPlanPreference> FindManualActionPlanPreferences(IManualActionPlanPreferenceStore store, ManualActionPlanPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
