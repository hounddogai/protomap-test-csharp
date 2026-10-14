namespace Acme.Workforce.Compensation.Benchmark.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark checkpoints by code.</summary>
public interface IBenchmarkCheckpointStore
{
    BenchmarkCheckpoint? Find(string code);
    void Save(BenchmarkCheckpoint item);
    bool Remove(string code);
    List<BenchmarkCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkCheckpointStore(Dictionary<string, BenchmarkCheckpoint> items, List<string> order) : IBenchmarkCheckpointStore
{
    public Dictionary<string, BenchmarkCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkCheckpoint item)
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

    public List<BenchmarkCheckpoint> List()
    {
        var outValue = new List<BenchmarkCheckpoint>();
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
public sealed class CachedBenchmarkCheckpointStore(MemoryBenchmarkCheckpointStore inner, BenchmarkCheckpointCache cache, int hits, int misses) : IBenchmarkCheckpointStore
{
    public MemoryBenchmarkCheckpointStore Inner { get; set; } = inner;
    public BenchmarkCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkCheckpoint? Find(string code)
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

    public void Save(BenchmarkCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark checkpoints. Empty fields match all.</summary>
public sealed record BenchmarkCheckpointQuery(string Name, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkCheckpoint item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BenchmarkCheckpoint> Apply(List<BenchmarkCheckpoint> items)
    {
        var matched = Collections.Filter(items, (BenchmarkCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark checkpoint store.</summary>
public static class BenchmarkCheckpointStore
{
    public static IBenchmarkCheckpointStore NewMemoryBenchmarkCheckpointStore()
    {
        return new MemoryBenchmarkCheckpointStore(new Dictionary<string, BenchmarkCheckpoint>(), new List<string>());
    }

    public static IBenchmarkCheckpointStore NewCachedBenchmarkCheckpointStore(MemoryBenchmarkCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkCheckpointStore(inner, BenchmarkCheckpointCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BenchmarkCheckpoint> FindBenchmarkCheckpoints(IBenchmarkCheckpointStore store, BenchmarkCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
