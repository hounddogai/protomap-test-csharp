namespace Acme.Workforce.Workplace.AccessBadge.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge rollups by code.</summary>
public interface IAccessBadgeRollupStore
{
    AccessBadgeRollup? Find(string code);
    void Save(AccessBadgeRollup item);
    bool Remove(string code);
    List<AccessBadgeRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeRollupStore(Dictionary<string, AccessBadgeRollup> items, List<string> order) : IAccessBadgeRollupStore
{
    public Dictionary<string, AccessBadgeRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeRollup item)
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

    public List<AccessBadgeRollup> List()
    {
        var outValue = new List<AccessBadgeRollup>();
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
public sealed class CachedAccessBadgeRollupStore(MemoryAccessBadgeRollupStore inner, AccessBadgeRollupCache cache, int hits, int misses) : IAccessBadgeRollupStore
{
    public MemoryAccessBadgeRollupStore Inner { get; set; } = inner;
    public AccessBadgeRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeRollup? Find(string code)
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

    public void Save(AccessBadgeRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge rollups. Empty fields match all.</summary>
public sealed record AccessBadgeRollupQuery(string Country, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeRollup item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccessBadgeRollup> Apply(List<AccessBadgeRollup> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge rollup store.</summary>
public static class AccessBadgeRollupStore
{
    public static IAccessBadgeRollupStore NewMemoryAccessBadgeRollupStore()
    {
        return new MemoryAccessBadgeRollupStore(new Dictionary<string, AccessBadgeRollup>(), new List<string>());
    }

    public static IAccessBadgeRollupStore NewCachedAccessBadgeRollupStore(MemoryAccessBadgeRollupStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeRollupStore(inner, AccessBadgeRollupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeRollup> FindAccessBadgeRollups(IAccessBadgeRollupStore store, AccessBadgeRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
