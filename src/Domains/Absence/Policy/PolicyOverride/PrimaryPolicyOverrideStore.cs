namespace Acme.Workforce.Absence.Policy.PolicyOverride;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary policy overrides by code.</summary>
public interface IPrimaryPolicyOverrideStore
{
    PrimaryPolicyOverride? Find(string code);
    void Save(PrimaryPolicyOverride item);
    bool Remove(string code);
    List<PrimaryPolicyOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryPolicyOverrideStore(Dictionary<string, PrimaryPolicyOverride> items, List<string> order) : IPrimaryPolicyOverrideStore
{
    public Dictionary<string, PrimaryPolicyOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryPolicyOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryPolicyOverride item)
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

    public List<PrimaryPolicyOverride> List()
    {
        var outValue = new List<PrimaryPolicyOverride>();
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
public sealed class CachedPrimaryPolicyOverrideStore(MemoryPrimaryPolicyOverrideStore inner, PrimaryPolicyOverrideCache cache, int hits, int misses) : IPrimaryPolicyOverrideStore
{
    public MemoryPrimaryPolicyOverrideStore Inner { get; set; } = inner;
    public PrimaryPolicyOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryPolicyOverride? Find(string code)
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

    public void Save(PrimaryPolicyOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryPolicyOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary policy overrides. Empty fields match all.</summary>
public sealed record PrimaryPolicyOverrideQuery(string Region, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryPolicyOverride item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryPolicyOverride> Apply(List<PrimaryPolicyOverride> items)
    {
        var matched = Collections.Filter(items, (PrimaryPolicyOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary policy override store.</summary>
public static class PrimaryPolicyOverrideStore
{
    public static IPrimaryPolicyOverrideStore NewMemoryPrimaryPolicyOverrideStore()
    {
        return new MemoryPrimaryPolicyOverrideStore(
            new Dictionary<string, PrimaryPolicyOverride>(),
            new List<string>());
    }

    public static IPrimaryPolicyOverrideStore NewCachedPrimaryPolicyOverrideStore(MemoryPrimaryPolicyOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryPolicyOverrideStore(inner, PrimaryPolicyOverrideCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PrimaryPolicyOverride> FindPrimaryPolicyOverrides(IPrimaryPolicyOverrideStore store, PrimaryPolicyOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
