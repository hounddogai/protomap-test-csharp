namespace Acme.Workforce.Talent.SuccessionPlan.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan preferences by code.</summary>
public interface ISuccessionPlanPreferenceStore
{
    SuccessionPlanPreference? Find(string code);
    void Save(SuccessionPlanPreference item);
    bool Remove(string code);
    List<SuccessionPlanPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanPreferenceStore(Dictionary<string, SuccessionPlanPreference> items, List<string> order) : ISuccessionPlanPreferenceStore
{
    public Dictionary<string, SuccessionPlanPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanPreference item)
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

    public List<SuccessionPlanPreference> List()
    {
        var outValue = new List<SuccessionPlanPreference>();
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
public sealed class CachedSuccessionPlanPreferenceStore(MemorySuccessionPlanPreferenceStore inner, SuccessionPlanPreferenceCache cache, int hits, int misses) : ISuccessionPlanPreferenceStore
{
    public MemorySuccessionPlanPreferenceStore Inner { get; set; } = inner;
    public SuccessionPlanPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanPreference? Find(string code)
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

    public void Save(SuccessionPlanPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan preferences. Empty fields match all.</summary>
public sealed record SuccessionPlanPreferenceQuery(string Team, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanPreference item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SuccessionPlanPreference> Apply(List<SuccessionPlanPreference> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan preference store.</summary>
public static class SuccessionPlanPreferenceStore
{
    public static ISuccessionPlanPreferenceStore NewMemorySuccessionPlanPreferenceStore()
    {
        return new MemorySuccessionPlanPreferenceStore(
            new Dictionary<string, SuccessionPlanPreference>(),
            new List<string>());
    }

    public static ISuccessionPlanPreferenceStore NewCachedSuccessionPlanPreferenceStore(MemorySuccessionPlanPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanPreferenceStore(
            inner,
            SuccessionPlanPreferenceCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<SuccessionPlanPreference> FindSuccessionPlanPreferences(ISuccessionPlanPreferenceStore store, SuccessionPlanPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
