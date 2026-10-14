namespace Acme.Workforce.Contractors.Engagement.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps engagement preferences by code.</summary>
public interface IEngagementPreferenceStore
{
    EngagementPreference? Find(string code);
    void Save(EngagementPreference item);
    bool Remove(string code);
    List<EngagementPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEngagementPreferenceStore(Dictionary<string, EngagementPreference> items, List<string> order) : IEngagementPreferenceStore
{
    public Dictionary<string, EngagementPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EngagementPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EngagementPreference item)
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

    public List<EngagementPreference> List()
    {
        var outValue = new List<EngagementPreference>();
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
public sealed class CachedEngagementPreferenceStore(MemoryEngagementPreferenceStore inner, EngagementPreferenceCache cache, int hits, int misses) : IEngagementPreferenceStore
{
    public MemoryEngagementPreferenceStore Inner { get; set; } = inner;
    public EngagementPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EngagementPreference? Find(string code)
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

    public void Save(EngagementPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EngagementPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects engagement preferences. Empty fields match all.</summary>
public sealed record EngagementPreferenceQuery(string Department, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(EngagementPreference item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EngagementPreference> Apply(List<EngagementPreference> items)
    {
        var matched = Collections.Filter(items, (EngagementPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of engagement preference store.</summary>
public static class EngagementPreferenceStore
{
    public static IEngagementPreferenceStore NewMemoryEngagementPreferenceStore()
    {
        return new MemoryEngagementPreferenceStore(
            new Dictionary<string, EngagementPreference>(),
            new List<string>());
    }

    public static IEngagementPreferenceStore NewCachedEngagementPreferenceStore(MemoryEngagementPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedEngagementPreferenceStore(inner, EngagementPreferenceCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<EngagementPreference> FindEngagementPreferences(IEngagementPreferenceStore store, EngagementPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
