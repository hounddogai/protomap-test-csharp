namespace Acme.Workforce.Compensation.Benchmark.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark milestones by code.</summary>
public interface IBenchmarkMilestoneStore
{
    BenchmarkMilestone? Find(string code);
    void Save(BenchmarkMilestone item);
    bool Remove(string code);
    List<BenchmarkMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkMilestoneStore(Dictionary<string, BenchmarkMilestone> items, List<string> order) : IBenchmarkMilestoneStore
{
    public Dictionary<string, BenchmarkMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkMilestone item)
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

    public List<BenchmarkMilestone> List()
    {
        var outValue = new List<BenchmarkMilestone>();
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
public sealed class CachedBenchmarkMilestoneStore(MemoryBenchmarkMilestoneStore inner, BenchmarkMilestoneCache cache, int hits, int misses) : IBenchmarkMilestoneStore
{
    public MemoryBenchmarkMilestoneStore Inner { get; set; } = inner;
    public BenchmarkMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkMilestone? Find(string code)
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

    public void Save(BenchmarkMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark milestones. Empty fields match all.</summary>
public sealed record BenchmarkMilestoneQuery(string Label, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkMilestone item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BenchmarkMilestone> Apply(List<BenchmarkMilestone> items)
    {
        var matched = Collections.Filter(items, (BenchmarkMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark milestone store.</summary>
public static class BenchmarkMilestoneStore
{
    public static IBenchmarkMilestoneStore NewMemoryBenchmarkMilestoneStore()
    {
        return new MemoryBenchmarkMilestoneStore(new Dictionary<string, BenchmarkMilestone>(), new List<string>());
    }

    public static IBenchmarkMilestoneStore NewCachedBenchmarkMilestoneStore(MemoryBenchmarkMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkMilestoneStore(inner, BenchmarkMilestoneCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BenchmarkMilestone> FindBenchmarkMilestones(IBenchmarkMilestoneStore store, BenchmarkMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
