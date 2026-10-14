namespace Acme.Workforce.Workplace.AccessBadge.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge targets by code.</summary>
public interface IAccessBadgeTargetStore
{
    AccessBadgeTarget? Find(string code);
    void Save(AccessBadgeTarget item);
    bool Remove(string code);
    List<AccessBadgeTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeTargetStore(Dictionary<string, AccessBadgeTarget> items, List<string> order) : IAccessBadgeTargetStore
{
    public Dictionary<string, AccessBadgeTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeTarget item)
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

    public List<AccessBadgeTarget> List()
    {
        var outValue = new List<AccessBadgeTarget>();
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
public sealed class CachedAccessBadgeTargetStore(MemoryAccessBadgeTargetStore inner, AccessBadgeTargetCache cache, int hits, int misses) : IAccessBadgeTargetStore
{
    public MemoryAccessBadgeTargetStore Inner { get; set; } = inner;
    public AccessBadgeTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeTarget? Find(string code)
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

    public void Save(AccessBadgeTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge targets. Empty fields match all.</summary>
public sealed record AccessBadgeTargetQuery(string Region, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeTarget item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccessBadgeTarget> Apply(List<AccessBadgeTarget> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge target store.</summary>
public static class AccessBadgeTargetStore
{
    public static IAccessBadgeTargetStore NewMemoryAccessBadgeTargetStore()
    {
        return new MemoryAccessBadgeTargetStore(new Dictionary<string, AccessBadgeTarget>(), new List<string>());
    }

    public static IAccessBadgeTargetStore NewCachedAccessBadgeTargetStore(MemoryAccessBadgeTargetStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeTargetStore(inner, AccessBadgeTargetCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeTarget> FindAccessBadgeTargets(IAccessBadgeTargetStore store, AccessBadgeTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
