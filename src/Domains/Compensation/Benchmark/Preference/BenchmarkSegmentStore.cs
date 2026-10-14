namespace Acme.Workforce.Compensation.Benchmark.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark segments by code.</summary>
public interface IBenchmarkSegmentStore
{
    BenchmarkSegment? Find(string code);
    void Save(BenchmarkSegment item);
    bool Remove(string code);
    List<BenchmarkSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkSegmentStore(Dictionary<string, BenchmarkSegment> items, List<string> order) : IBenchmarkSegmentStore
{
    public Dictionary<string, BenchmarkSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkSegment item)
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

    public List<BenchmarkSegment> List()
    {
        var outValue = new List<BenchmarkSegment>();
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
public sealed class CachedBenchmarkSegmentStore(MemoryBenchmarkSegmentStore inner, BenchmarkSegmentCache cache, int hits, int misses) : IBenchmarkSegmentStore
{
    public MemoryBenchmarkSegmentStore Inner { get; set; } = inner;
    public BenchmarkSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkSegment? Find(string code)
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

    public void Save(BenchmarkSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark segments. Empty fields match all.</summary>
public sealed record BenchmarkSegmentQuery(string Label, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkSegment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BenchmarkSegment> Apply(List<BenchmarkSegment> items)
    {
        var matched = Collections.Filter(items, (BenchmarkSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark segment store.</summary>
public static class BenchmarkSegmentStore
{
    public static IBenchmarkSegmentStore NewMemoryBenchmarkSegmentStore()
    {
        return new MemoryBenchmarkSegmentStore(new Dictionary<string, BenchmarkSegment>(), new List<string>());
    }

    public static IBenchmarkSegmentStore NewCachedBenchmarkSegmentStore(MemoryBenchmarkSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkSegmentStore(inner, BenchmarkSegmentCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BenchmarkSegment> FindBenchmarkSegments(IBenchmarkSegmentStore store, BenchmarkSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
