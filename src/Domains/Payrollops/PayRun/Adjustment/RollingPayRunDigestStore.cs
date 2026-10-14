namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling pay run digests by code.</summary>
public interface IRollingPayRunDigestStore
{
    RollingPayRunDigest? Find(string code);
    void Save(RollingPayRunDigest item);
    bool Remove(string code);
    List<RollingPayRunDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingPayRunDigestStore(Dictionary<string, RollingPayRunDigest> items, List<string> order) : IRollingPayRunDigestStore
{
    public Dictionary<string, RollingPayRunDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingPayRunDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingPayRunDigest item)
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

    public List<RollingPayRunDigest> List()
    {
        var outValue = new List<RollingPayRunDigest>();
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
public sealed class CachedRollingPayRunDigestStore(MemoryRollingPayRunDigestStore inner, RollingPayRunDigestCache cache, int hits, int misses) : IRollingPayRunDigestStore
{
    public MemoryRollingPayRunDigestStore Inner { get; set; } = inner;
    public RollingPayRunDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingPayRunDigest? Find(string code)
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

    public void Save(RollingPayRunDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingPayRunDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling pay run digests. Empty fields match all.</summary>
public sealed record RollingPayRunDigestQuery(string Country, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(RollingPayRunDigest item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingPayRunDigest> Apply(List<RollingPayRunDigest> items)
    {
        var matched = Collections.Filter(items, (RollingPayRunDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling pay run digest store.</summary>
public static class RollingPayRunDigestStore
{
    public static IRollingPayRunDigestStore NewMemoryRollingPayRunDigestStore()
    {
        return new MemoryRollingPayRunDigestStore(new Dictionary<string, RollingPayRunDigest>(), new List<string>());
    }

    public static IRollingPayRunDigestStore NewCachedRollingPayRunDigestStore(MemoryRollingPayRunDigestStore inner, int ttl, IClock clock)
    {
        return new CachedRollingPayRunDigestStore(inner, RollingPayRunDigestCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RollingPayRunDigest> FindRollingPayRunDigests(IRollingPayRunDigestStore store, RollingPayRunDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
