namespace Acme.Workforce.Compliance.Exception.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception checkpoints by code.</summary>
public interface IExceptionCheckpointStore
{
    ExceptionCheckpoint? Find(string code);
    void Save(ExceptionCheckpoint item);
    bool Remove(string code);
    List<ExceptionCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionCheckpointStore(Dictionary<string, ExceptionCheckpoint> items, List<string> order) : IExceptionCheckpointStore
{
    public Dictionary<string, ExceptionCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionCheckpoint item)
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

    public List<ExceptionCheckpoint> List()
    {
        var outValue = new List<ExceptionCheckpoint>();
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
public sealed class CachedExceptionCheckpointStore(MemoryExceptionCheckpointStore inner, ExceptionCheckpointCache cache, int hits, int misses) : IExceptionCheckpointStore
{
    public MemoryExceptionCheckpointStore Inner { get; set; } = inner;
    public ExceptionCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionCheckpoint? Find(string code)
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

    public void Save(ExceptionCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception checkpoints. Empty fields match all.</summary>
public sealed record ExceptionCheckpointQuery(string Label, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionCheckpoint item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ExceptionCheckpoint> Apply(List<ExceptionCheckpoint> items)
    {
        var matched = Collections.Filter(items, (ExceptionCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception checkpoint store.</summary>
public static class ExceptionCheckpointStore
{
    public static IExceptionCheckpointStore NewMemoryExceptionCheckpointStore()
    {
        return new MemoryExceptionCheckpointStore(new Dictionary<string, ExceptionCheckpoint>(), new List<string>());
    }

    public static IExceptionCheckpointStore NewCachedExceptionCheckpointStore(MemoryExceptionCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionCheckpointStore(inner, ExceptionCheckpointCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ExceptionCheckpoint> FindExceptionCheckpoints(IExceptionCheckpointStore store, ExceptionCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
