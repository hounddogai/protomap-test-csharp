namespace Acme.Workforce.Contractors.StatementOfWork.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional statement of work digests by code.</summary>
public interface IRegionalStatementOfWorkDigestStore
{
    RegionalStatementOfWorkDigest? Find(string code);
    void Save(RegionalStatementOfWorkDigest item);
    bool Remove(string code);
    List<RegionalStatementOfWorkDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalStatementOfWorkDigestStore(Dictionary<string, RegionalStatementOfWorkDigest> items, List<string> order) : IRegionalStatementOfWorkDigestStore
{
    public Dictionary<string, RegionalStatementOfWorkDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalStatementOfWorkDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalStatementOfWorkDigest item)
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

    public List<RegionalStatementOfWorkDigest> List()
    {
        var outValue = new List<RegionalStatementOfWorkDigest>();
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
public sealed class CachedRegionalStatementOfWorkDigestStore(MemoryRegionalStatementOfWorkDigestStore inner, RegionalStatementOfWorkDigestCache cache, int hits, int misses) : IRegionalStatementOfWorkDigestStore
{
    public MemoryRegionalStatementOfWorkDigestStore Inner { get; set; } = inner;
    public RegionalStatementOfWorkDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalStatementOfWorkDigest? Find(string code)
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

    public void Save(RegionalStatementOfWorkDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalStatementOfWorkDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional statement of work digests. Empty fields match all.</summary>
public sealed record RegionalStatementOfWorkDigestQuery(string Channel, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalStatementOfWorkDigest item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalStatementOfWorkDigest> Apply(List<RegionalStatementOfWorkDigest> items)
    {
        var matched = Collections.Filter(items, (RegionalStatementOfWorkDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional statement of work digest store.</summary>
public static class RegionalStatementOfWorkDigestStore
{
    public static IRegionalStatementOfWorkDigestStore NewMemoryRegionalStatementOfWorkDigestStore()
    {
        return new MemoryRegionalStatementOfWorkDigestStore(
            new Dictionary<string, RegionalStatementOfWorkDigest>(),
            new List<string>());
    }

    public static IRegionalStatementOfWorkDigestStore NewCachedRegionalStatementOfWorkDigestStore(MemoryRegionalStatementOfWorkDigestStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalStatementOfWorkDigestStore(
            inner,
            RegionalStatementOfWorkDigestCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RegionalStatementOfWorkDigest> FindRegionalStatementOfWorkDigests(IRegionalStatementOfWorkDigestStore store, RegionalStatementOfWorkDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
