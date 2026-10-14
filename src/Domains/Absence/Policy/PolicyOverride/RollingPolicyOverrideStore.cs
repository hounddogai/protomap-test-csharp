namespace Acme.Workforce.Absence.Policy.PolicyOverride;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling policy overrides by code.</summary>
public interface IRollingPolicyOverrideStore
{
    RollingPolicyOverride? Find(string code);
    void Save(RollingPolicyOverride item);
    bool Remove(string code);
    List<RollingPolicyOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingPolicyOverrideStore(Dictionary<string, RollingPolicyOverride> items, List<string> order) : IRollingPolicyOverrideStore
{
    public Dictionary<string, RollingPolicyOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingPolicyOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingPolicyOverride item)
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

    public List<RollingPolicyOverride> List()
    {
        var outValue = new List<RollingPolicyOverride>();
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
public sealed class CachedRollingPolicyOverrideStore(MemoryRollingPolicyOverrideStore inner, RollingPolicyOverrideCache cache, int hits, int misses) : IRollingPolicyOverrideStore
{
    public MemoryRollingPolicyOverrideStore Inner { get; set; } = inner;
    public RollingPolicyOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingPolicyOverride? Find(string code)
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

    public void Save(RollingPolicyOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingPolicyOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling policy overrides. Empty fields match all.</summary>
public sealed record RollingPolicyOverrideQuery(string Country, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(RollingPolicyOverride item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingPolicyOverride> Apply(List<RollingPolicyOverride> items)
    {
        var matched = Collections.Filter(items, (RollingPolicyOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling policy override store.</summary>
public static class RollingPolicyOverrideStore
{
    public static IRollingPolicyOverrideStore NewMemoryRollingPolicyOverrideStore()
    {
        return new MemoryRollingPolicyOverrideStore(
            new Dictionary<string, RollingPolicyOverride>(),
            new List<string>());
    }

    public static IRollingPolicyOverrideStore NewCachedRollingPolicyOverrideStore(MemoryRollingPolicyOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedRollingPolicyOverrideStore(inner, RollingPolicyOverrideCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RollingPolicyOverride> FindRollingPolicyOverrides(IRollingPolicyOverrideStore store, RollingPolicyOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
