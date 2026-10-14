namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered benchmark adjustments by code.</summary>
public interface ITieredBenchmarkAdjustmentStore
{
    TieredBenchmarkAdjustment? Find(string code);
    void Save(TieredBenchmarkAdjustment item);
    bool Remove(string code);
    List<TieredBenchmarkAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredBenchmarkAdjustmentStore(Dictionary<string, TieredBenchmarkAdjustment> items, List<string> order) : ITieredBenchmarkAdjustmentStore
{
    public Dictionary<string, TieredBenchmarkAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredBenchmarkAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredBenchmarkAdjustment item)
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

    public List<TieredBenchmarkAdjustment> List()
    {
        var outValue = new List<TieredBenchmarkAdjustment>();
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
public sealed class CachedTieredBenchmarkAdjustmentStore(MemoryTieredBenchmarkAdjustmentStore inner, TieredBenchmarkAdjustmentCache cache, int hits, int misses) : ITieredBenchmarkAdjustmentStore
{
    public MemoryTieredBenchmarkAdjustmentStore Inner { get; set; } = inner;
    public TieredBenchmarkAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredBenchmarkAdjustment? Find(string code)
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

    public void Save(TieredBenchmarkAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredBenchmarkAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered benchmark adjustments. Empty fields match all.</summary>
public sealed record TieredBenchmarkAdjustmentQuery(string Name, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(TieredBenchmarkAdjustment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredBenchmarkAdjustment> Apply(List<TieredBenchmarkAdjustment> items)
    {
        var matched = Collections.Filter(items, (TieredBenchmarkAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered benchmark adjustment store.</summary>
public static class TieredBenchmarkAdjustmentStore
{
    public static ITieredBenchmarkAdjustmentStore NewMemoryTieredBenchmarkAdjustmentStore()
    {
        return new MemoryTieredBenchmarkAdjustmentStore(
            new Dictionary<string, TieredBenchmarkAdjustment>(),
            new List<string>());
    }

    public static ITieredBenchmarkAdjustmentStore NewCachedTieredBenchmarkAdjustmentStore(MemoryTieredBenchmarkAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedTieredBenchmarkAdjustmentStore(
            inner,
            TieredBenchmarkAdjustmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<TieredBenchmarkAdjustment> FindTieredBenchmarkAdjustments(ITieredBenchmarkAdjustmentStore store, TieredBenchmarkAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
