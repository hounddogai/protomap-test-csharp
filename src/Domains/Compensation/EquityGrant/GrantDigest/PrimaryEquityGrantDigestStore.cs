namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary equity grant digests by code.</summary>
public interface IPrimaryEquityGrantDigestStore
{
    PrimaryEquityGrantDigest? Find(string code);
    void Save(PrimaryEquityGrantDigest item);
    bool Remove(string code);
    List<PrimaryEquityGrantDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryEquityGrantDigestStore(Dictionary<string, PrimaryEquityGrantDigest> items, List<string> order) : IPrimaryEquityGrantDigestStore
{
    public Dictionary<string, PrimaryEquityGrantDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryEquityGrantDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryEquityGrantDigest item)
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

    public List<PrimaryEquityGrantDigest> List()
    {
        var outValue = new List<PrimaryEquityGrantDigest>();
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
public sealed class CachedPrimaryEquityGrantDigestStore(MemoryPrimaryEquityGrantDigestStore inner, PrimaryEquityGrantDigestCache cache, int hits, int misses) : IPrimaryEquityGrantDigestStore
{
    public MemoryPrimaryEquityGrantDigestStore Inner { get; set; } = inner;
    public PrimaryEquityGrantDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryEquityGrantDigest? Find(string code)
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

    public void Save(PrimaryEquityGrantDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryEquityGrantDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary equity grant digests. Empty fields match all.</summary>
public sealed record PrimaryEquityGrantDigestQuery(string Channel, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryEquityGrantDigest item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryEquityGrantDigest> Apply(List<PrimaryEquityGrantDigest> items)
    {
        var matched = Collections.Filter(items, (PrimaryEquityGrantDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary equity grant digest store.</summary>
public static class PrimaryEquityGrantDigestStore
{
    public static IPrimaryEquityGrantDigestStore NewMemoryPrimaryEquityGrantDigestStore()
    {
        return new MemoryPrimaryEquityGrantDigestStore(
            new Dictionary<string, PrimaryEquityGrantDigest>(),
            new List<string>());
    }

    public static IPrimaryEquityGrantDigestStore NewCachedPrimaryEquityGrantDigestStore(MemoryPrimaryEquityGrantDigestStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryEquityGrantDigestStore(
            inner,
            PrimaryEquityGrantDigestCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryEquityGrantDigest> FindPrimaryEquityGrantDigests(IPrimaryEquityGrantDigestStore store, PrimaryEquityGrantDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
