namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark budgets by code.</summary>
public interface IBenchmarkBudgetStore
{
    BenchmarkBudget? Find(string code);
    void Save(BenchmarkBudget item);
    bool Remove(string code);
    List<BenchmarkBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkBudgetStore(Dictionary<string, BenchmarkBudget> items, List<string> order) : IBenchmarkBudgetStore
{
    public Dictionary<string, BenchmarkBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkBudget item)
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

    public List<BenchmarkBudget> List()
    {
        var outValue = new List<BenchmarkBudget>();
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
public sealed class CachedBenchmarkBudgetStore(MemoryBenchmarkBudgetStore inner, BenchmarkBudgetCache cache, int hits, int misses) : IBenchmarkBudgetStore
{
    public MemoryBenchmarkBudgetStore Inner { get; set; } = inner;
    public BenchmarkBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkBudget? Find(string code)
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

    public void Save(BenchmarkBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark budgets. Empty fields match all.</summary>
public sealed record BenchmarkBudgetQuery(string Segment, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkBudget item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BenchmarkBudget> Apply(List<BenchmarkBudget> items)
    {
        var matched = Collections.Filter(items, (BenchmarkBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark budget store.</summary>
public static class BenchmarkBudgetStore
{
    public static IBenchmarkBudgetStore NewMemoryBenchmarkBudgetStore()
    {
        return new MemoryBenchmarkBudgetStore(new Dictionary<string, BenchmarkBudget>(), new List<string>());
    }

    public static IBenchmarkBudgetStore NewCachedBenchmarkBudgetStore(MemoryBenchmarkBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkBudgetStore(inner, BenchmarkBudgetCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BenchmarkBudget> FindBenchmarkBudgets(IBenchmarkBudgetStore store, BenchmarkBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
