namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback statement of work checkpoints by code.</summary>
public interface IFallbackStatementOfWorkCheckpointStore
{
    FallbackStatementOfWorkCheckpoint? Find(string code);
    void Save(FallbackStatementOfWorkCheckpoint item);
    bool Remove(string code);
    List<FallbackStatementOfWorkCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackStatementOfWorkCheckpointStore(Dictionary<string, FallbackStatementOfWorkCheckpoint> items, List<string> order) : IFallbackStatementOfWorkCheckpointStore
{
    public Dictionary<string, FallbackStatementOfWorkCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackStatementOfWorkCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackStatementOfWorkCheckpoint item)
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

    public List<FallbackStatementOfWorkCheckpoint> List()
    {
        var outValue = new List<FallbackStatementOfWorkCheckpoint>();
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
public sealed class CachedFallbackStatementOfWorkCheckpointStore(MemoryFallbackStatementOfWorkCheckpointStore inner, FallbackStatementOfWorkCheckpointCache cache, int hits, int misses) : IFallbackStatementOfWorkCheckpointStore
{
    public MemoryFallbackStatementOfWorkCheckpointStore Inner { get; set; } = inner;
    public FallbackStatementOfWorkCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackStatementOfWorkCheckpoint? Find(string code)
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

    public void Save(FallbackStatementOfWorkCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackStatementOfWorkCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback statement of work checkpoints. Empty fields match all.</summary>
public sealed record FallbackStatementOfWorkCheckpointQuery(string Unit, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackStatementOfWorkCheckpoint item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackStatementOfWorkCheckpoint> Apply(List<FallbackStatementOfWorkCheckpoint> items)
    {
        var matched = Collections.Filter(items, (FallbackStatementOfWorkCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback statement of work checkpoint store.</summary>
public static class FallbackStatementOfWorkCheckpointStore
{
    public static IFallbackStatementOfWorkCheckpointStore NewMemoryFallbackStatementOfWorkCheckpointStore()
    {
        return new MemoryFallbackStatementOfWorkCheckpointStore(
            new Dictionary<string, FallbackStatementOfWorkCheckpoint>(),
            new List<string>());
    }

    public static IFallbackStatementOfWorkCheckpointStore NewCachedFallbackStatementOfWorkCheckpointStore(MemoryFallbackStatementOfWorkCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackStatementOfWorkCheckpointStore(
            inner,
            FallbackStatementOfWorkCheckpointCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<FallbackStatementOfWorkCheckpoint> FindFallbackStatementOfWorkCheckpoints(IFallbackStatementOfWorkCheckpointStore store, FallbackStatementOfWorkCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
