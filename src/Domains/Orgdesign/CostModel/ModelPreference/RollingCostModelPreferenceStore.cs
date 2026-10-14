namespace Acme.Workforce.Orgdesign.CostModel.ModelPreference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling cost model preferences by code.</summary>
public interface IRollingCostModelPreferenceStore
{
    RollingCostModelPreference? Find(string code);
    void Save(RollingCostModelPreference item);
    bool Remove(string code);
    List<RollingCostModelPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingCostModelPreferenceStore(Dictionary<string, RollingCostModelPreference> items, List<string> order) : IRollingCostModelPreferenceStore
{
    public Dictionary<string, RollingCostModelPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingCostModelPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingCostModelPreference item)
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

    public List<RollingCostModelPreference> List()
    {
        var outValue = new List<RollingCostModelPreference>();
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
public sealed class CachedRollingCostModelPreferenceStore(MemoryRollingCostModelPreferenceStore inner, RollingCostModelPreferenceCache cache, int hits, int misses) : IRollingCostModelPreferenceStore
{
    public MemoryRollingCostModelPreferenceStore Inner { get; set; } = inner;
    public RollingCostModelPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingCostModelPreference? Find(string code)
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

    public void Save(RollingCostModelPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingCostModelPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling cost model preferences. Empty fields match all.</summary>
public sealed record RollingCostModelPreferenceQuery(string Region, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(RollingCostModelPreference item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingCostModelPreference> Apply(List<RollingCostModelPreference> items)
    {
        var matched = Collections.Filter(items, (RollingCostModelPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling cost model preference store.</summary>
public static class RollingCostModelPreferenceStore
{
    public static IRollingCostModelPreferenceStore NewMemoryRollingCostModelPreferenceStore()
    {
        return new MemoryRollingCostModelPreferenceStore(
            new Dictionary<string, RollingCostModelPreference>(),
            new List<string>());
    }

    public static IRollingCostModelPreferenceStore NewCachedRollingCostModelPreferenceStore(MemoryRollingCostModelPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedRollingCostModelPreferenceStore(
            inner,
            RollingCostModelPreferenceCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RollingCostModelPreference> FindRollingCostModelPreferences(IRollingCostModelPreferenceStore store, RollingCostModelPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
