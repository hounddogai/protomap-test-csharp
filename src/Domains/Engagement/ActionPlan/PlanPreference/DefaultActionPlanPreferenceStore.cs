namespace Acme.Workforce.Engagement.ActionPlan.PlanPreference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default action plan preferences by code.</summary>
public interface IDefaultActionPlanPreferenceStore
{
    DefaultActionPlanPreference? Find(string code);
    void Save(DefaultActionPlanPreference item);
    bool Remove(string code);
    List<DefaultActionPlanPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultActionPlanPreferenceStore(Dictionary<string, DefaultActionPlanPreference> items, List<string> order) : IDefaultActionPlanPreferenceStore
{
    public Dictionary<string, DefaultActionPlanPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultActionPlanPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultActionPlanPreference item)
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

    public List<DefaultActionPlanPreference> List()
    {
        var outValue = new List<DefaultActionPlanPreference>();
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
public sealed class CachedDefaultActionPlanPreferenceStore(MemoryDefaultActionPlanPreferenceStore inner, DefaultActionPlanPreferenceCache cache, int hits, int misses) : IDefaultActionPlanPreferenceStore
{
    public MemoryDefaultActionPlanPreferenceStore Inner { get; set; } = inner;
    public DefaultActionPlanPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultActionPlanPreference? Find(string code)
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

    public void Save(DefaultActionPlanPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultActionPlanPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default action plan preferences. Empty fields match all.</summary>
public sealed record DefaultActionPlanPreferenceQuery(string Source, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultActionPlanPreference item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<DefaultActionPlanPreference> Apply(List<DefaultActionPlanPreference> items)
    {
        var matched = Collections.Filter(items, (DefaultActionPlanPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default action plan preference store.</summary>
public static class DefaultActionPlanPreferenceStore
{
    public static IDefaultActionPlanPreferenceStore NewMemoryDefaultActionPlanPreferenceStore()
    {
        return new MemoryDefaultActionPlanPreferenceStore(
            new Dictionary<string, DefaultActionPlanPreference>(),
            new List<string>());
    }

    public static IDefaultActionPlanPreferenceStore NewCachedDefaultActionPlanPreferenceStore(MemoryDefaultActionPlanPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultActionPlanPreferenceStore(
            inner,
            DefaultActionPlanPreferenceCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<DefaultActionPlanPreference> FindDefaultActionPlanPreferences(IDefaultActionPlanPreferenceStore store, DefaultActionPlanPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
