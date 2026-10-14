namespace Acme.Workforce.Absence.Balance.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default balance preferences by code.</summary>
public interface IDefaultBalancePreferenceStore
{
    DefaultBalancePreference? Find(string code);
    void Save(DefaultBalancePreference item);
    bool Remove(string code);
    List<DefaultBalancePreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultBalancePreferenceStore(Dictionary<string, DefaultBalancePreference> items, List<string> order) : IDefaultBalancePreferenceStore
{
    public Dictionary<string, DefaultBalancePreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultBalancePreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultBalancePreference item)
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

    public List<DefaultBalancePreference> List()
    {
        var outValue = new List<DefaultBalancePreference>();
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
public sealed class CachedDefaultBalancePreferenceStore(MemoryDefaultBalancePreferenceStore inner, DefaultBalancePreferenceCache cache, int hits, int misses) : IDefaultBalancePreferenceStore
{
    public MemoryDefaultBalancePreferenceStore Inner { get; set; } = inner;
    public DefaultBalancePreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultBalancePreference? Find(string code)
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

    public void Save(DefaultBalancePreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultBalancePreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default balance preferences. Empty fields match all.</summary>
public sealed record DefaultBalancePreferenceQuery(string Label, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultBalancePreference item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultBalancePreference> Apply(List<DefaultBalancePreference> items)
    {
        var matched = Collections.Filter(items, (DefaultBalancePreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default balance preference store.</summary>
public static class DefaultBalancePreferenceStore
{
    public static IDefaultBalancePreferenceStore NewMemoryDefaultBalancePreferenceStore()
    {
        return new MemoryDefaultBalancePreferenceStore(
            new Dictionary<string, DefaultBalancePreference>(),
            new List<string>());
    }

    public static IDefaultBalancePreferenceStore NewCachedDefaultBalancePreferenceStore(MemoryDefaultBalancePreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultBalancePreferenceStore(
            inner,
            DefaultBalancePreferenceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<DefaultBalancePreference> FindDefaultBalancePreferences(IDefaultBalancePreferenceStore store, DefaultBalancePreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
