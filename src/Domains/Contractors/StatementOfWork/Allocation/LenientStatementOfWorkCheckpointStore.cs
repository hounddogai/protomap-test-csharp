namespace Acme.Workforce.Contractors.StatementOfWork.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient statement of work checkpoints by code.</summary>
public interface ILenientStatementOfWorkCheckpointStore
{
    LenientStatementOfWorkCheckpoint? Find(string code);
    void Save(LenientStatementOfWorkCheckpoint item);
    bool Remove(string code);
    List<LenientStatementOfWorkCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientStatementOfWorkCheckpointStore(Dictionary<string, LenientStatementOfWorkCheckpoint> items, List<string> order) : ILenientStatementOfWorkCheckpointStore
{
    public Dictionary<string, LenientStatementOfWorkCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientStatementOfWorkCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientStatementOfWorkCheckpoint item)
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

    public List<LenientStatementOfWorkCheckpoint> List()
    {
        var outValue = new List<LenientStatementOfWorkCheckpoint>();
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
public sealed class CachedLenientStatementOfWorkCheckpointStore(MemoryLenientStatementOfWorkCheckpointStore inner, LenientStatementOfWorkCheckpointCache cache, int hits, int misses) : ILenientStatementOfWorkCheckpointStore
{
    public MemoryLenientStatementOfWorkCheckpointStore Inner { get; set; } = inner;
    public LenientStatementOfWorkCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientStatementOfWorkCheckpoint? Find(string code)
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

    public void Save(LenientStatementOfWorkCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientStatementOfWorkCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient statement of work checkpoints. Empty fields match all.</summary>
public sealed record LenientStatementOfWorkCheckpointQuery(string Name, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(LenientStatementOfWorkCheckpoint item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientStatementOfWorkCheckpoint> Apply(List<LenientStatementOfWorkCheckpoint> items)
    {
        var matched = Collections.Filter(items, (LenientStatementOfWorkCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient statement of work checkpoint store.</summary>
public static class LenientStatementOfWorkCheckpointStore
{
    public static ILenientStatementOfWorkCheckpointStore NewMemoryLenientStatementOfWorkCheckpointStore()
    {
        return new MemoryLenientStatementOfWorkCheckpointStore(
            new Dictionary<string, LenientStatementOfWorkCheckpoint>(),
            new List<string>());
    }

    public static ILenientStatementOfWorkCheckpointStore NewCachedLenientStatementOfWorkCheckpointStore(MemoryLenientStatementOfWorkCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedLenientStatementOfWorkCheckpointStore(
            inner,
            LenientStatementOfWorkCheckpointCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LenientStatementOfWorkCheckpoint> FindLenientStatementOfWorkCheckpoints(ILenientStatementOfWorkCheckpointStore store, LenientStatementOfWorkCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
