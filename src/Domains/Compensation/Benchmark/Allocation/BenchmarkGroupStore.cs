namespace Acme.Workforce.Compensation.Benchmark.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark groups by code.</summary>
public interface IBenchmarkGroupStore
{
    BenchmarkGroup? Find(string code);
    void Save(BenchmarkGroup item);
    bool Remove(string code);
    List<BenchmarkGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkGroupStore(Dictionary<string, BenchmarkGroup> items, List<string> order) : IBenchmarkGroupStore
{
    public Dictionary<string, BenchmarkGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkGroup item)
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

    public List<BenchmarkGroup> List()
    {
        var outValue = new List<BenchmarkGroup>();
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
public sealed class CachedBenchmarkGroupStore(MemoryBenchmarkGroupStore inner, BenchmarkGroupCache cache, int hits, int misses) : IBenchmarkGroupStore
{
    public MemoryBenchmarkGroupStore Inner { get; set; } = inner;
    public BenchmarkGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkGroup? Find(string code)
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

    public void Save(BenchmarkGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark groups. Empty fields match all.</summary>
public sealed record BenchmarkGroupQuery(string Name, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkGroup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<BenchmarkGroup> Apply(List<BenchmarkGroup> items)
    {
        var matched = Collections.Filter(items, (BenchmarkGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark group store.</summary>
public static class BenchmarkGroupStore
{
    public static IBenchmarkGroupStore NewMemoryBenchmarkGroupStore()
    {
        return new MemoryBenchmarkGroupStore(new Dictionary<string, BenchmarkGroup>(), new List<string>());
    }

    public static IBenchmarkGroupStore NewCachedBenchmarkGroupStore(MemoryBenchmarkGroupStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkGroupStore(inner, BenchmarkGroupCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BenchmarkGroup> FindBenchmarkGroups(IBenchmarkGroupStore store, BenchmarkGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
