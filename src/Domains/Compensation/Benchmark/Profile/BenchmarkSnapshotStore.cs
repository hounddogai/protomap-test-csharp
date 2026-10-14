namespace Acme.Workforce.Compensation.Benchmark.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark snapshots by code.</summary>
public interface IBenchmarkSnapshotStore
{
    BenchmarkSnapshot? Find(string code);
    void Save(BenchmarkSnapshot item);
    bool Remove(string code);
    List<BenchmarkSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkSnapshotStore(Dictionary<string, BenchmarkSnapshot> items, List<string> order) : IBenchmarkSnapshotStore
{
    public Dictionary<string, BenchmarkSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkSnapshot item)
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

    public List<BenchmarkSnapshot> List()
    {
        var outValue = new List<BenchmarkSnapshot>();
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
public sealed class CachedBenchmarkSnapshotStore(MemoryBenchmarkSnapshotStore inner, BenchmarkSnapshotCache cache, int hits, int misses) : IBenchmarkSnapshotStore
{
    public MemoryBenchmarkSnapshotStore Inner { get; set; } = inner;
    public BenchmarkSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkSnapshot? Find(string code)
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

    public void Save(BenchmarkSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark snapshots. Empty fields match all.</summary>
public sealed record BenchmarkSnapshotQuery(string Category, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkSnapshot item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BenchmarkSnapshot> Apply(List<BenchmarkSnapshot> items)
    {
        var matched = Collections.Filter(items, (BenchmarkSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark snapshot store.</summary>
public static class BenchmarkSnapshotStore
{
    public static IBenchmarkSnapshotStore NewMemoryBenchmarkSnapshotStore()
    {
        return new MemoryBenchmarkSnapshotStore(new Dictionary<string, BenchmarkSnapshot>(), new List<string>());
    }

    public static IBenchmarkSnapshotStore NewCachedBenchmarkSnapshotStore(MemoryBenchmarkSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkSnapshotStore(inner, BenchmarkSnapshotCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BenchmarkSnapshot> FindBenchmarkSnapshots(IBenchmarkSnapshotStore store, BenchmarkSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
