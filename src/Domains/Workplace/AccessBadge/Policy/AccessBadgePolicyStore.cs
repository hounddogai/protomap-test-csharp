namespace Acme.Workforce.Workplace.AccessBadge.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge policies by code.</summary>
public interface IAccessBadgePolicyStore
{
    AccessBadgePolicy? Find(string code);
    void Save(AccessBadgePolicy item);
    bool Remove(string code);
    List<AccessBadgePolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgePolicyStore(Dictionary<string, AccessBadgePolicy> items, List<string> order) : IAccessBadgePolicyStore
{
    public Dictionary<string, AccessBadgePolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgePolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgePolicy item)
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

    public List<AccessBadgePolicy> List()
    {
        var outValue = new List<AccessBadgePolicy>();
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
public sealed class CachedAccessBadgePolicyStore(MemoryAccessBadgePolicyStore inner, AccessBadgePolicyCache cache, int hits, int misses) : IAccessBadgePolicyStore
{
    public MemoryAccessBadgePolicyStore Inner { get; set; } = inner;
    public AccessBadgePolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgePolicy? Find(string code)
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

    public void Save(AccessBadgePolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgePolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge policies. Empty fields match all.</summary>
public sealed record AccessBadgePolicyQuery(string Segment, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgePolicy item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<AccessBadgePolicy> Apply(List<AccessBadgePolicy> items)
    {
        var matched = Collections.Filter(items, (AccessBadgePolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge policy store.</summary>
public static class AccessBadgePolicyStore
{
    public static IAccessBadgePolicyStore NewMemoryAccessBadgePolicyStore()
    {
        return new MemoryAccessBadgePolicyStore(new Dictionary<string, AccessBadgePolicy>(), new List<string>());
    }

    public static IAccessBadgePolicyStore NewCachedAccessBadgePolicyStore(MemoryAccessBadgePolicyStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgePolicyStore(inner, AccessBadgePolicyCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AccessBadgePolicy> FindAccessBadgePolicies(IAccessBadgePolicyStore store, AccessBadgePolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
