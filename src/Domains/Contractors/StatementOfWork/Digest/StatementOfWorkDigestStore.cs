namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work digests by code.</summary>
public interface IStatementOfWorkDigestStore
{
    StatementOfWorkDigest? Find(string code);
    void Save(StatementOfWorkDigest item);
    bool Remove(string code);
    List<StatementOfWorkDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkDigestStore(Dictionary<string, StatementOfWorkDigest> items, List<string> order) : IStatementOfWorkDigestStore
{
    public Dictionary<string, StatementOfWorkDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkDigest item)
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

    public List<StatementOfWorkDigest> List()
    {
        var outValue = new List<StatementOfWorkDigest>();
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
public sealed class CachedStatementOfWorkDigestStore(MemoryStatementOfWorkDigestStore inner, StatementOfWorkDigestCache cache, int hits, int misses) : IStatementOfWorkDigestStore
{
    public MemoryStatementOfWorkDigestStore Inner { get; set; } = inner;
    public StatementOfWorkDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkDigest? Find(string code)
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

    public void Save(StatementOfWorkDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work digests. Empty fields match all.</summary>
public sealed record StatementOfWorkDigestQuery(string Region, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkDigest item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<StatementOfWorkDigest> Apply(List<StatementOfWorkDigest> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work digest store.</summary>
public static class StatementOfWorkDigestStore
{
    public static IStatementOfWorkDigestStore NewMemoryStatementOfWorkDigestStore()
    {
        return new MemoryStatementOfWorkDigestStore(
            new Dictionary<string, StatementOfWorkDigest>(),
            new List<string>());
    }

    public static IStatementOfWorkDigestStore NewCachedStatementOfWorkDigestStore(MemoryStatementOfWorkDigestStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkDigestStore(inner, StatementOfWorkDigestCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<StatementOfWorkDigest> FindStatementOfWorkDigests(IStatementOfWorkDigestStore store, StatementOfWorkDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
