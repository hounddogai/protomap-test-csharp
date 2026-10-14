namespace Acme.Workforce.Compensation.Benchmark.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted benchmark baselines by code.</summary>
public interface IWeightedBenchmarkBaselineStore
{
    WeightedBenchmarkBaseline? Find(string code);
    void Save(WeightedBenchmarkBaseline item);
    bool Remove(string code);
    List<WeightedBenchmarkBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedBenchmarkBaselineStore(Dictionary<string, WeightedBenchmarkBaseline> items, List<string> order) : IWeightedBenchmarkBaselineStore
{
    public Dictionary<string, WeightedBenchmarkBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedBenchmarkBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedBenchmarkBaseline item)
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

    public List<WeightedBenchmarkBaseline> List()
    {
        var outValue = new List<WeightedBenchmarkBaseline>();
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
public sealed class CachedWeightedBenchmarkBaselineStore(MemoryWeightedBenchmarkBaselineStore inner, WeightedBenchmarkBaselineCache cache, int hits, int misses) : IWeightedBenchmarkBaselineStore
{
    public MemoryWeightedBenchmarkBaselineStore Inner { get; set; } = inner;
    public WeightedBenchmarkBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedBenchmarkBaseline? Find(string code)
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

    public void Save(WeightedBenchmarkBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedBenchmarkBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted benchmark baselines. Empty fields match all.</summary>
public sealed record WeightedBenchmarkBaselineQuery(string Segment, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedBenchmarkBaseline item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedBenchmarkBaseline> Apply(List<WeightedBenchmarkBaseline> items)
    {
        var matched = Collections.Filter(items, (WeightedBenchmarkBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted benchmark baseline store.</summary>
public static class WeightedBenchmarkBaselineStore
{
    public static IWeightedBenchmarkBaselineStore NewMemoryWeightedBenchmarkBaselineStore()
    {
        return new MemoryWeightedBenchmarkBaselineStore(
            new Dictionary<string, WeightedBenchmarkBaseline>(),
            new List<string>());
    }

    public static IWeightedBenchmarkBaselineStore NewCachedWeightedBenchmarkBaselineStore(MemoryWeightedBenchmarkBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedBenchmarkBaselineStore(
            inner,
            WeightedBenchmarkBaselineCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<WeightedBenchmarkBaseline> FindWeightedBenchmarkBaselines(IWeightedBenchmarkBaselineStore store, WeightedBenchmarkBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
