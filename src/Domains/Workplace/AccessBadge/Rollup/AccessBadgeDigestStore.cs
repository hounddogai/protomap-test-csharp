namespace Acme.Workforce.Workplace.AccessBadge.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge digests by code.</summary>
public interface IAccessBadgeDigestStore
{
    AccessBadgeDigest? Find(string code);
    void Save(AccessBadgeDigest item);
    bool Remove(string code);
    List<AccessBadgeDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeDigestStore(Dictionary<string, AccessBadgeDigest> items, List<string> order) : IAccessBadgeDigestStore
{
    public Dictionary<string, AccessBadgeDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeDigest item)
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

    public List<AccessBadgeDigest> List()
    {
        var outValue = new List<AccessBadgeDigest>();
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
public sealed class CachedAccessBadgeDigestStore(MemoryAccessBadgeDigestStore inner, AccessBadgeDigestCache cache, int hits, int misses) : IAccessBadgeDigestStore
{
    public MemoryAccessBadgeDigestStore Inner { get; set; } = inner;
    public AccessBadgeDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeDigest? Find(string code)
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

    public void Save(AccessBadgeDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge digests. Empty fields match all.</summary>
public sealed record AccessBadgeDigestQuery(string Name, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeDigest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccessBadgeDigest> Apply(List<AccessBadgeDigest> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge digest store.</summary>
public static class AccessBadgeDigestStore
{
    public static IAccessBadgeDigestStore NewMemoryAccessBadgeDigestStore()
    {
        return new MemoryAccessBadgeDigestStore(new Dictionary<string, AccessBadgeDigest>(), new List<string>());
    }

    public static IAccessBadgeDigestStore NewCachedAccessBadgeDigestStore(MemoryAccessBadgeDigestStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeDigestStore(inner, AccessBadgeDigestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeDigest> FindAccessBadgeDigests(IAccessBadgeDigestStore store, AccessBadgeDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
