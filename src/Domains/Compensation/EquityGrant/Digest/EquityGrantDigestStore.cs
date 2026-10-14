namespace Acme.Workforce.Compensation.EquityGrant.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant digests by code.</summary>
public interface IEquityGrantDigestStore
{
    EquityGrantDigest? Find(string code);
    void Save(EquityGrantDigest item);
    bool Remove(string code);
    List<EquityGrantDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantDigestStore(Dictionary<string, EquityGrantDigest> items, List<string> order) : IEquityGrantDigestStore
{
    public Dictionary<string, EquityGrantDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantDigest item)
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

    public List<EquityGrantDigest> List()
    {
        var outValue = new List<EquityGrantDigest>();
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
public sealed class CachedEquityGrantDigestStore(MemoryEquityGrantDigestStore inner, EquityGrantDigestCache cache, int hits, int misses) : IEquityGrantDigestStore
{
    public MemoryEquityGrantDigestStore Inner { get; set; } = inner;
    public EquityGrantDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantDigest? Find(string code)
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

    public void Save(EquityGrantDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant digests. Empty fields match all.</summary>
public sealed record EquityGrantDigestQuery(string Channel, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantDigest item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantDigest> Apply(List<EquityGrantDigest> items)
    {
        var matched = Collections.Filter(items, (EquityGrantDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant digest store.</summary>
public static class EquityGrantDigestStore
{
    public static IEquityGrantDigestStore NewMemoryEquityGrantDigestStore()
    {
        return new MemoryEquityGrantDigestStore(new Dictionary<string, EquityGrantDigest>(), new List<string>());
    }

    public static IEquityGrantDigestStore NewCachedEquityGrantDigestStore(MemoryEquityGrantDigestStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantDigestStore(inner, EquityGrantDigestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<EquityGrantDigest> FindEquityGrantDigests(IEquityGrantDigestStore store, EquityGrantDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
