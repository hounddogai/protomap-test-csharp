namespace Acme.Workforce.Workplace.AccessBadge.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge tiers by code.</summary>
public interface IAccessBadgeTierStore
{
    AccessBadgeTier? Find(string code);
    void Save(AccessBadgeTier item);
    bool Remove(string code);
    List<AccessBadgeTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeTierStore(Dictionary<string, AccessBadgeTier> items, List<string> order) : IAccessBadgeTierStore
{
    public Dictionary<string, AccessBadgeTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeTier item)
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

    public List<AccessBadgeTier> List()
    {
        var outValue = new List<AccessBadgeTier>();
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
public sealed class CachedAccessBadgeTierStore(MemoryAccessBadgeTierStore inner, AccessBadgeTierCache cache, int hits, int misses) : IAccessBadgeTierStore
{
    public MemoryAccessBadgeTierStore Inner { get; set; } = inner;
    public AccessBadgeTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeTier? Find(string code)
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

    public void Save(AccessBadgeTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge tiers. Empty fields match all.</summary>
public sealed record AccessBadgeTierQuery(string Region, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeTier item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccessBadgeTier> Apply(List<AccessBadgeTier> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge tier store.</summary>
public static class AccessBadgeTierStore
{
    public static IAccessBadgeTierStore NewMemoryAccessBadgeTierStore()
    {
        return new MemoryAccessBadgeTierStore(new Dictionary<string, AccessBadgeTier>(), new List<string>());
    }

    public static IAccessBadgeTierStore NewCachedAccessBadgeTierStore(MemoryAccessBadgeTierStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeTierStore(inner, AccessBadgeTierCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeTier> FindAccessBadgeTiers(IAccessBadgeTierStore store, AccessBadgeTierQuery query)
    {
        return query.Apply(store.List());
    }
}
