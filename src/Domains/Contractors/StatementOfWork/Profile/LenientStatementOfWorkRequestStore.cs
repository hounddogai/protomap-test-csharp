namespace Acme.Workforce.Contractors.StatementOfWork.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient statement of work requests by code.</summary>
public interface ILenientStatementOfWorkRequestStore
{
    LenientStatementOfWorkRequest? Find(string code);
    void Save(LenientStatementOfWorkRequest item);
    bool Remove(string code);
    List<LenientStatementOfWorkRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientStatementOfWorkRequestStore(Dictionary<string, LenientStatementOfWorkRequest> items, List<string> order) : ILenientStatementOfWorkRequestStore
{
    public Dictionary<string, LenientStatementOfWorkRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientStatementOfWorkRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientStatementOfWorkRequest item)
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

    public List<LenientStatementOfWorkRequest> List()
    {
        var outValue = new List<LenientStatementOfWorkRequest>();
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
public sealed class CachedLenientStatementOfWorkRequestStore(MemoryLenientStatementOfWorkRequestStore inner, LenientStatementOfWorkRequestCache cache, int hits, int misses) : ILenientStatementOfWorkRequestStore
{
    public MemoryLenientStatementOfWorkRequestStore Inner { get; set; } = inner;
    public LenientStatementOfWorkRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientStatementOfWorkRequest? Find(string code)
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

    public void Save(LenientStatementOfWorkRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientStatementOfWorkRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient statement of work requests. Empty fields match all.</summary>
public sealed record LenientStatementOfWorkRequestQuery(string Source, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(LenientStatementOfWorkRequest item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<LenientStatementOfWorkRequest> Apply(List<LenientStatementOfWorkRequest> items)
    {
        var matched = Collections.Filter(items, (LenientStatementOfWorkRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient statement of work request store.</summary>
public static class LenientStatementOfWorkRequestStore
{
    public static ILenientStatementOfWorkRequestStore NewMemoryLenientStatementOfWorkRequestStore()
    {
        return new MemoryLenientStatementOfWorkRequestStore(
            new Dictionary<string, LenientStatementOfWorkRequest>(),
            new List<string>());
    }

    public static ILenientStatementOfWorkRequestStore NewCachedLenientStatementOfWorkRequestStore(MemoryLenientStatementOfWorkRequestStore inner, int ttl, IClock clock)
    {
        return new CachedLenientStatementOfWorkRequestStore(
            inner,
            LenientStatementOfWorkRequestCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LenientStatementOfWorkRequest> FindLenientStatementOfWorkRequests(ILenientStatementOfWorkRequestStore store, LenientStatementOfWorkRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
