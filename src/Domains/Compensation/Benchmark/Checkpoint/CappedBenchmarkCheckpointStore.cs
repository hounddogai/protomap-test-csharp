namespace Acme.Workforce.Compensation.Benchmark.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped benchmark checkpoints by code.</summary>
public interface ICappedBenchmarkCheckpointStore
{
    CappedBenchmarkCheckpoint? Find(string code);
    void Save(CappedBenchmarkCheckpoint item);
    bool Remove(string code);
    List<CappedBenchmarkCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedBenchmarkCheckpointStore(Dictionary<string, CappedBenchmarkCheckpoint> items, List<string> order) : ICappedBenchmarkCheckpointStore
{
    public Dictionary<string, CappedBenchmarkCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedBenchmarkCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedBenchmarkCheckpoint item)
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

    public List<CappedBenchmarkCheckpoint> List()
    {
        var outValue = new List<CappedBenchmarkCheckpoint>();
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
public sealed class CachedCappedBenchmarkCheckpointStore(MemoryCappedBenchmarkCheckpointStore inner, CappedBenchmarkCheckpointCache cache, int hits, int misses) : ICappedBenchmarkCheckpointStore
{
    public MemoryCappedBenchmarkCheckpointStore Inner { get; set; } = inner;
    public CappedBenchmarkCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedBenchmarkCheckpoint? Find(string code)
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

    public void Save(CappedBenchmarkCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedBenchmarkCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped benchmark checkpoints. Empty fields match all.</summary>
public sealed record CappedBenchmarkCheckpointQuery(string Category, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(CappedBenchmarkCheckpoint item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedBenchmarkCheckpoint> Apply(List<CappedBenchmarkCheckpoint> items)
    {
        var matched = Collections.Filter(items, (CappedBenchmarkCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped benchmark checkpoint store.</summary>
public static class CappedBenchmarkCheckpointStore
{
    public static ICappedBenchmarkCheckpointStore NewMemoryCappedBenchmarkCheckpointStore()
    {
        return new MemoryCappedBenchmarkCheckpointStore(
            new Dictionary<string, CappedBenchmarkCheckpoint>(),
            new List<string>());
    }

    public static ICappedBenchmarkCheckpointStore NewCachedCappedBenchmarkCheckpointStore(MemoryCappedBenchmarkCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedCappedBenchmarkCheckpointStore(
            inner,
            CappedBenchmarkCheckpointCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CappedBenchmarkCheckpoint> FindCappedBenchmarkCheckpoints(ICappedBenchmarkCheckpointStore store, CappedBenchmarkCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
