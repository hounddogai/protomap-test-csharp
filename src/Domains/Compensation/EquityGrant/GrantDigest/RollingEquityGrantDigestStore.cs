namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling equity grant digests by code.</summary>
public interface IRollingEquityGrantDigestStore
{
    RollingEquityGrantDigest? Find(string code);
    void Save(RollingEquityGrantDigest item);
    bool Remove(string code);
    List<RollingEquityGrantDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingEquityGrantDigestStore(Dictionary<string, RollingEquityGrantDigest> items, List<string> order) : IRollingEquityGrantDigestStore
{
    public Dictionary<string, RollingEquityGrantDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingEquityGrantDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingEquityGrantDigest item)
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

    public List<RollingEquityGrantDigest> List()
    {
        var outValue = new List<RollingEquityGrantDigest>();
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
public sealed class CachedRollingEquityGrantDigestStore(MemoryRollingEquityGrantDigestStore inner, RollingEquityGrantDigestCache cache, int hits, int misses) : IRollingEquityGrantDigestStore
{
    public MemoryRollingEquityGrantDigestStore Inner { get; set; } = inner;
    public RollingEquityGrantDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingEquityGrantDigest? Find(string code)
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

    public void Save(RollingEquityGrantDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingEquityGrantDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling equity grant digests. Empty fields match all.</summary>
public sealed record RollingEquityGrantDigestQuery(string Category, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(RollingEquityGrantDigest item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingEquityGrantDigest> Apply(List<RollingEquityGrantDigest> items)
    {
        var matched = Collections.Filter(items, (RollingEquityGrantDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling equity grant digest store.</summary>
public static class RollingEquityGrantDigestStore
{
    public static IRollingEquityGrantDigestStore NewMemoryRollingEquityGrantDigestStore()
    {
        return new MemoryRollingEquityGrantDigestStore(
            new Dictionary<string, RollingEquityGrantDigest>(),
            new List<string>());
    }

    public static IRollingEquityGrantDigestStore NewCachedRollingEquityGrantDigestStore(MemoryRollingEquityGrantDigestStore inner, int ttl, IClock clock)
    {
        return new CachedRollingEquityGrantDigestStore(
            inner,
            RollingEquityGrantDigestCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<RollingEquityGrantDigest> FindRollingEquityGrantDigests(IRollingEquityGrantDigestStore store, RollingEquityGrantDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
