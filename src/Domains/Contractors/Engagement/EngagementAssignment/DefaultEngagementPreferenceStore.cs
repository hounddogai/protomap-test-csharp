namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default engagement preferences by code.</summary>
public interface IDefaultEngagementPreferenceStore
{
    DefaultEngagementPreference? Find(string code);
    void Save(DefaultEngagementPreference item);
    bool Remove(string code);
    List<DefaultEngagementPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultEngagementPreferenceStore(Dictionary<string, DefaultEngagementPreference> items, List<string> order) : IDefaultEngagementPreferenceStore
{
    public Dictionary<string, DefaultEngagementPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultEngagementPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultEngagementPreference item)
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

    public List<DefaultEngagementPreference> List()
    {
        var outValue = new List<DefaultEngagementPreference>();
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
public sealed class CachedDefaultEngagementPreferenceStore(MemoryDefaultEngagementPreferenceStore inner, DefaultEngagementPreferenceCache cache, int hits, int misses) : IDefaultEngagementPreferenceStore
{
    public MemoryDefaultEngagementPreferenceStore Inner { get; set; } = inner;
    public DefaultEngagementPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultEngagementPreference? Find(string code)
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

    public void Save(DefaultEngagementPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultEngagementPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default engagement preferences. Empty fields match all.</summary>
public sealed record DefaultEngagementPreferenceQuery(string Name, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultEngagementPreference item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultEngagementPreference> Apply(List<DefaultEngagementPreference> items)
    {
        var matched = Collections.Filter(items, (DefaultEngagementPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default engagement preference store.</summary>
public static class DefaultEngagementPreferenceStore
{
    public static IDefaultEngagementPreferenceStore NewMemoryDefaultEngagementPreferenceStore()
    {
        return new MemoryDefaultEngagementPreferenceStore(
            new Dictionary<string, DefaultEngagementPreference>(),
            new List<string>());
    }

    public static IDefaultEngagementPreferenceStore NewCachedDefaultEngagementPreferenceStore(MemoryDefaultEngagementPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultEngagementPreferenceStore(
            inner,
            DefaultEngagementPreferenceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<DefaultEngagementPreference> FindDefaultEngagementPreferences(IDefaultEngagementPreferenceStore store, DefaultEngagementPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
