namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work checkpoints by code.</summary>
public interface IStatementOfWorkCheckpointStore
{
    StatementOfWorkCheckpoint? Find(string code);
    void Save(StatementOfWorkCheckpoint item);
    bool Remove(string code);
    List<StatementOfWorkCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkCheckpointStore(Dictionary<string, StatementOfWorkCheckpoint> items, List<string> order) : IStatementOfWorkCheckpointStore
{
    public Dictionary<string, StatementOfWorkCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkCheckpoint item)
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

    public List<StatementOfWorkCheckpoint> List()
    {
        var outValue = new List<StatementOfWorkCheckpoint>();
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
public sealed class CachedStatementOfWorkCheckpointStore(MemoryStatementOfWorkCheckpointStore inner, StatementOfWorkCheckpointCache cache, int hits, int misses) : IStatementOfWorkCheckpointStore
{
    public MemoryStatementOfWorkCheckpointStore Inner { get; set; } = inner;
    public StatementOfWorkCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkCheckpoint? Find(string code)
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

    public void Save(StatementOfWorkCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work checkpoints. Empty fields match all.</summary>
public sealed record StatementOfWorkCheckpointQuery(string Title, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkCheckpoint item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkCheckpoint> Apply(List<StatementOfWorkCheckpoint> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work checkpoint store.</summary>
public static class StatementOfWorkCheckpointStore
{
    public static IStatementOfWorkCheckpointStore NewMemoryStatementOfWorkCheckpointStore()
    {
        return new MemoryStatementOfWorkCheckpointStore(
            new Dictionary<string, StatementOfWorkCheckpoint>(),
            new List<string>());
    }

    public static IStatementOfWorkCheckpointStore NewCachedStatementOfWorkCheckpointStore(MemoryStatementOfWorkCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkCheckpointStore(
            inner,
            StatementOfWorkCheckpointCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StatementOfWorkCheckpoint> FindStatementOfWorkCheckpoints(IStatementOfWorkCheckpointStore store, StatementOfWorkCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
