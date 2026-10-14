namespace Acme.Workforce.Compensation.Benchmark.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered benchmark summaries by code.</summary>
public interface ITieredBenchmarkSummaryStore
{
    TieredBenchmarkSummary? Find(string code);
    void Save(TieredBenchmarkSummary item);
    bool Remove(string code);
    List<TieredBenchmarkSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredBenchmarkSummaryStore(Dictionary<string, TieredBenchmarkSummary> items, List<string> order) : ITieredBenchmarkSummaryStore
{
    public Dictionary<string, TieredBenchmarkSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredBenchmarkSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredBenchmarkSummary item)
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

    public List<TieredBenchmarkSummary> List()
    {
        var outValue = new List<TieredBenchmarkSummary>();
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
public sealed class CachedTieredBenchmarkSummaryStore(MemoryTieredBenchmarkSummaryStore inner, TieredBenchmarkSummaryCache cache, int hits, int misses) : ITieredBenchmarkSummaryStore
{
    public MemoryTieredBenchmarkSummaryStore Inner { get; set; } = inner;
    public TieredBenchmarkSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredBenchmarkSummary? Find(string code)
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

    public void Save(TieredBenchmarkSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredBenchmarkSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered benchmark summaries. Empty fields match all.</summary>
public sealed record TieredBenchmarkSummaryQuery(string Name, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(TieredBenchmarkSummary item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredBenchmarkSummary> Apply(List<TieredBenchmarkSummary> items)
    {
        var matched = Collections.Filter(items, (TieredBenchmarkSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered benchmark summary store.</summary>
public static class TieredBenchmarkSummaryStore
{
    public static ITieredBenchmarkSummaryStore NewMemoryTieredBenchmarkSummaryStore()
    {
        return new MemoryTieredBenchmarkSummaryStore(
            new Dictionary<string, TieredBenchmarkSummary>(),
            new List<string>());
    }

    public static ITieredBenchmarkSummaryStore NewCachedTieredBenchmarkSummaryStore(MemoryTieredBenchmarkSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedTieredBenchmarkSummaryStore(
            inner,
            TieredBenchmarkSummaryCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<TieredBenchmarkSummary> FindTieredBenchmarkSummaries(ITieredBenchmarkSummaryStore store, TieredBenchmarkSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
