namespace Acme.Workforce.Compensation.Benchmark.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark rules by code.</summary>
public interface IBenchmarkRuleStore
{
    BenchmarkRule? Find(string code);
    void Save(BenchmarkRule item);
    bool Remove(string code);
    List<BenchmarkRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkRuleStore(Dictionary<string, BenchmarkRule> items, List<string> order) : IBenchmarkRuleStore
{
    public Dictionary<string, BenchmarkRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkRule item)
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

    public List<BenchmarkRule> List()
    {
        var outValue = new List<BenchmarkRule>();
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
public sealed class CachedBenchmarkRuleStore(MemoryBenchmarkRuleStore inner, BenchmarkRuleCache cache, int hits, int misses) : IBenchmarkRuleStore
{
    public MemoryBenchmarkRuleStore Inner { get; set; } = inner;
    public BenchmarkRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkRule? Find(string code)
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

    public void Save(BenchmarkRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark rules. Empty fields match all.</summary>
public sealed record BenchmarkRuleQuery(string Label, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkRule item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BenchmarkRule> Apply(List<BenchmarkRule> items)
    {
        var matched = Collections.Filter(items, (BenchmarkRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark rule store.</summary>
public static class BenchmarkRuleStore
{
    public static IBenchmarkRuleStore NewMemoryBenchmarkRuleStore()
    {
        return new MemoryBenchmarkRuleStore(new Dictionary<string, BenchmarkRule>(), new List<string>());
    }

    public static IBenchmarkRuleStore NewCachedBenchmarkRuleStore(MemoryBenchmarkRuleStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkRuleStore(inner, BenchmarkRuleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BenchmarkRule> FindBenchmarkRules(IBenchmarkRuleStore store, BenchmarkRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
