namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge claims by code.</summary>
public interface IAccessBadgeClaimStore
{
    AccessBadgeClaim? Find(string code);
    void Save(AccessBadgeClaim item);
    bool Remove(string code);
    List<AccessBadgeClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeClaimStore(Dictionary<string, AccessBadgeClaim> items, List<string> order) : IAccessBadgeClaimStore
{
    public Dictionary<string, AccessBadgeClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeClaim item)
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

    public List<AccessBadgeClaim> List()
    {
        var outValue = new List<AccessBadgeClaim>();
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
public sealed class CachedAccessBadgeClaimStore(MemoryAccessBadgeClaimStore inner, AccessBadgeClaimCache cache, int hits, int misses) : IAccessBadgeClaimStore
{
    public MemoryAccessBadgeClaimStore Inner { get; set; } = inner;
    public AccessBadgeClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeClaim? Find(string code)
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

    public void Save(AccessBadgeClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge claims. Empty fields match all.</summary>
public sealed record AccessBadgeClaimQuery(string Team, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeClaim item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccessBadgeClaim> Apply(List<AccessBadgeClaim> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge claim store.</summary>
public static class AccessBadgeClaimStore
{
    public static IAccessBadgeClaimStore NewMemoryAccessBadgeClaimStore()
    {
        return new MemoryAccessBadgeClaimStore(new Dictionary<string, AccessBadgeClaim>(), new List<string>());
    }

    public static IAccessBadgeClaimStore NewCachedAccessBadgeClaimStore(MemoryAccessBadgeClaimStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeClaimStore(inner, AccessBadgeClaimCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeClaim> FindAccessBadgeClaims(IAccessBadgeClaimStore store, AccessBadgeClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
