namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling benchmark summaries by code.</summary>
public interface IRollingBenchmarkSummaryStore
{
    RollingBenchmarkSummary? Find(string code);
    void Save(RollingBenchmarkSummary item);
    bool Remove(string code);
    List<RollingBenchmarkSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingBenchmarkSummaryStore(Dictionary<string, RollingBenchmarkSummary> items, List<string> order) : IRollingBenchmarkSummaryStore
{
    public Dictionary<string, RollingBenchmarkSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingBenchmarkSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingBenchmarkSummary item)
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

    public List<RollingBenchmarkSummary> List()
    {
        var outValue = new List<RollingBenchmarkSummary>();
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
public sealed class CachedRollingBenchmarkSummaryStore(MemoryRollingBenchmarkSummaryStore inner, RollingBenchmarkSummaryCache cache, int hits, int misses) : IRollingBenchmarkSummaryStore
{
    public MemoryRollingBenchmarkSummaryStore Inner { get; set; } = inner;
    public RollingBenchmarkSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingBenchmarkSummary? Find(string code)
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

    public void Save(RollingBenchmarkSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingBenchmarkSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling benchmark summaries. Empty fields match all.</summary>
public sealed record RollingBenchmarkSummaryQuery(string Region, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(RollingBenchmarkSummary item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingBenchmarkSummary> Apply(List<RollingBenchmarkSummary> items)
    {
        var matched = Collections.Filter(items, (RollingBenchmarkSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling benchmark summary store.</summary>
public static class RollingBenchmarkSummaryStore
{
    public static IRollingBenchmarkSummaryStore NewMemoryRollingBenchmarkSummaryStore()
    {
        return new MemoryRollingBenchmarkSummaryStore(
            new Dictionary<string, RollingBenchmarkSummary>(),
            new List<string>());
    }

    public static IRollingBenchmarkSummaryStore NewCachedRollingBenchmarkSummaryStore(MemoryRollingBenchmarkSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedRollingBenchmarkSummaryStore(
            inner,
            RollingBenchmarkSummaryCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<RollingBenchmarkSummary> FindRollingBenchmarkSummaries(IRollingBenchmarkSummaryStore store, RollingBenchmarkSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
