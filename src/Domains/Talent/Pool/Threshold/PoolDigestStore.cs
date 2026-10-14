namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool digests by code.</summary>
public interface IPoolDigestStore
{
    PoolDigest? Find(string code);
    void Save(PoolDigest item);
    bool Remove(string code);
    List<PoolDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolDigestStore(Dictionary<string, PoolDigest> items, List<string> order) : IPoolDigestStore
{
    public Dictionary<string, PoolDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolDigest item)
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

    public List<PoolDigest> List()
    {
        var outValue = new List<PoolDigest>();
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
public sealed class CachedPoolDigestStore(MemoryPoolDigestStore inner, PoolDigestCache cache, int hits, int misses) : IPoolDigestStore
{
    public MemoryPoolDigestStore Inner { get; set; } = inner;
    public PoolDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolDigest? Find(string code)
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

    public void Save(PoolDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool digests. Empty fields match all.</summary>
public sealed record PoolDigestQuery(string Title, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(PoolDigest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolDigest> Apply(List<PoolDigest> items)
    {
        var matched = Collections.Filter(items, (PoolDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool digest store.</summary>
public static class PoolDigestStore
{
    public static IPoolDigestStore NewMemoryPoolDigestStore()
    {
        return new MemoryPoolDigestStore(new Dictionary<string, PoolDigest>(), new List<string>());
    }

    public static IPoolDigestStore NewCachedPoolDigestStore(MemoryPoolDigestStore inner, int ttl, IClock clock)
    {
        return new CachedPoolDigestStore(inner, PoolDigestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PoolDigest> FindPoolDigests(IPoolDigestStore store, PoolDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
