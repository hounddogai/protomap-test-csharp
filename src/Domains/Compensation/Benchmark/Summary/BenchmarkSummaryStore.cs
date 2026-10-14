namespace Acme.Workforce.Compensation.Benchmark.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark summaries by code.</summary>
public interface IBenchmarkSummaryStore
{
    BenchmarkSummary? Find(string code);
    void Save(BenchmarkSummary item);
    bool Remove(string code);
    List<BenchmarkSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkSummaryStore(Dictionary<string, BenchmarkSummary> items, List<string> order) : IBenchmarkSummaryStore
{
    public Dictionary<string, BenchmarkSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkSummary item)
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

    public List<BenchmarkSummary> List()
    {
        var outValue = new List<BenchmarkSummary>();
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
public sealed class CachedBenchmarkSummaryStore(MemoryBenchmarkSummaryStore inner, BenchmarkSummaryCache cache, int hits, int misses) : IBenchmarkSummaryStore
{
    public MemoryBenchmarkSummaryStore Inner { get; set; } = inner;
    public BenchmarkSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkSummary? Find(string code)
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

    public void Save(BenchmarkSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark summaries. Empty fields match all.</summary>
public sealed record BenchmarkSummaryQuery(string Label, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkSummary item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BenchmarkSummary> Apply(List<BenchmarkSummary> items)
    {
        var matched = Collections.Filter(items, (BenchmarkSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark summary store.</summary>
public static class BenchmarkSummaryStore
{
    public static IBenchmarkSummaryStore NewMemoryBenchmarkSummaryStore()
    {
        return new MemoryBenchmarkSummaryStore(new Dictionary<string, BenchmarkSummary>(), new List<string>());
    }

    public static IBenchmarkSummaryStore NewCachedBenchmarkSummaryStore(MemoryBenchmarkSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkSummaryStore(inner, BenchmarkSummaryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BenchmarkSummary> FindBenchmarkSummaries(IBenchmarkSummaryStore store, BenchmarkSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
