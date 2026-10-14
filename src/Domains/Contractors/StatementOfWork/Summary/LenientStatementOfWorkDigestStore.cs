namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient statement of work digests by code.</summary>
public interface ILenientStatementOfWorkDigestStore
{
    LenientStatementOfWorkDigest? Find(string code);
    void Save(LenientStatementOfWorkDigest item);
    bool Remove(string code);
    List<LenientStatementOfWorkDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientStatementOfWorkDigestStore(Dictionary<string, LenientStatementOfWorkDigest> items, List<string> order) : ILenientStatementOfWorkDigestStore
{
    public Dictionary<string, LenientStatementOfWorkDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientStatementOfWorkDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientStatementOfWorkDigest item)
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

    public List<LenientStatementOfWorkDigest> List()
    {
        var outValue = new List<LenientStatementOfWorkDigest>();
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
public sealed class CachedLenientStatementOfWorkDigestStore(MemoryLenientStatementOfWorkDigestStore inner, LenientStatementOfWorkDigestCache cache, int hits, int misses) : ILenientStatementOfWorkDigestStore
{
    public MemoryLenientStatementOfWorkDigestStore Inner { get; set; } = inner;
    public LenientStatementOfWorkDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientStatementOfWorkDigest? Find(string code)
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

    public void Save(LenientStatementOfWorkDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientStatementOfWorkDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient statement of work digests. Empty fields match all.</summary>
public sealed record LenientStatementOfWorkDigestQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(LenientStatementOfWorkDigest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientStatementOfWorkDigest> Apply(List<LenientStatementOfWorkDigest> items)
    {
        var matched = Collections.Filter(items, (LenientStatementOfWorkDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient statement of work digest store.</summary>
public static class LenientStatementOfWorkDigestStore
{
    public static ILenientStatementOfWorkDigestStore NewMemoryLenientStatementOfWorkDigestStore()
    {
        return new MemoryLenientStatementOfWorkDigestStore(
            new Dictionary<string, LenientStatementOfWorkDigest>(),
            new List<string>());
    }

    public static ILenientStatementOfWorkDigestStore NewCachedLenientStatementOfWorkDigestStore(MemoryLenientStatementOfWorkDigestStore inner, int ttl, IClock clock)
    {
        return new CachedLenientStatementOfWorkDigestStore(
            inner,
            LenientStatementOfWorkDigestCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LenientStatementOfWorkDigest> FindLenientStatementOfWorkDigests(ILenientStatementOfWorkDigestStore store, LenientStatementOfWorkDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
