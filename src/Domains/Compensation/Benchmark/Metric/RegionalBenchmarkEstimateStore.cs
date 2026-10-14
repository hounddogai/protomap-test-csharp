namespace Acme.Workforce.Compensation.Benchmark.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional benchmark estimates by code.</summary>
public interface IRegionalBenchmarkEstimateStore
{
    RegionalBenchmarkEstimate? Find(string code);
    void Save(RegionalBenchmarkEstimate item);
    bool Remove(string code);
    List<RegionalBenchmarkEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalBenchmarkEstimateStore(Dictionary<string, RegionalBenchmarkEstimate> items, List<string> order) : IRegionalBenchmarkEstimateStore
{
    public Dictionary<string, RegionalBenchmarkEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalBenchmarkEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalBenchmarkEstimate item)
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

    public List<RegionalBenchmarkEstimate> List()
    {
        var outValue = new List<RegionalBenchmarkEstimate>();
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
public sealed class CachedRegionalBenchmarkEstimateStore(MemoryRegionalBenchmarkEstimateStore inner, RegionalBenchmarkEstimateCache cache, int hits, int misses) : IRegionalBenchmarkEstimateStore
{
    public MemoryRegionalBenchmarkEstimateStore Inner { get; set; } = inner;
    public RegionalBenchmarkEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalBenchmarkEstimate? Find(string code)
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

    public void Save(RegionalBenchmarkEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalBenchmarkEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional benchmark estimates. Empty fields match all.</summary>
public sealed record RegionalBenchmarkEstimateQuery(string Unit, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalBenchmarkEstimate item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalBenchmarkEstimate> Apply(List<RegionalBenchmarkEstimate> items)
    {
        var matched = Collections.Filter(items, (RegionalBenchmarkEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional benchmark estimate store.</summary>
public static class RegionalBenchmarkEstimateStore
{
    public static IRegionalBenchmarkEstimateStore NewMemoryRegionalBenchmarkEstimateStore()
    {
        return new MemoryRegionalBenchmarkEstimateStore(
            new Dictionary<string, RegionalBenchmarkEstimate>(),
            new List<string>());
    }

    public static IRegionalBenchmarkEstimateStore NewCachedRegionalBenchmarkEstimateStore(MemoryRegionalBenchmarkEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalBenchmarkEstimateStore(
            inner,
            RegionalBenchmarkEstimateCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<RegionalBenchmarkEstimate> FindRegionalBenchmarkEstimates(IRegionalBenchmarkEstimateStore store, RegionalBenchmarkEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
