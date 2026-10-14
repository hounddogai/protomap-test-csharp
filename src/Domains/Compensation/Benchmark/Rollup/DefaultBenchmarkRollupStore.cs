namespace Acme.Workforce.Compensation.Benchmark.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default benchmark rollups by code.</summary>
public interface IDefaultBenchmarkRollupStore
{
    DefaultBenchmarkRollup? Find(string code);
    void Save(DefaultBenchmarkRollup item);
    bool Remove(string code);
    List<DefaultBenchmarkRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultBenchmarkRollupStore(Dictionary<string, DefaultBenchmarkRollup> items, List<string> order) : IDefaultBenchmarkRollupStore
{
    public Dictionary<string, DefaultBenchmarkRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultBenchmarkRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultBenchmarkRollup item)
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

    public List<DefaultBenchmarkRollup> List()
    {
        var outValue = new List<DefaultBenchmarkRollup>();
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
public sealed class CachedDefaultBenchmarkRollupStore(MemoryDefaultBenchmarkRollupStore inner, DefaultBenchmarkRollupCache cache, int hits, int misses) : IDefaultBenchmarkRollupStore
{
    public MemoryDefaultBenchmarkRollupStore Inner { get; set; } = inner;
    public DefaultBenchmarkRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultBenchmarkRollup? Find(string code)
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

    public void Save(DefaultBenchmarkRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultBenchmarkRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default benchmark rollups. Empty fields match all.</summary>
public sealed record DefaultBenchmarkRollupQuery(string Segment, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultBenchmarkRollup item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultBenchmarkRollup> Apply(List<DefaultBenchmarkRollup> items)
    {
        var matched = Collections.Filter(items, (DefaultBenchmarkRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default benchmark rollup store.</summary>
public static class DefaultBenchmarkRollupStore
{
    public static IDefaultBenchmarkRollupStore NewMemoryDefaultBenchmarkRollupStore()
    {
        return new MemoryDefaultBenchmarkRollupStore(
            new Dictionary<string, DefaultBenchmarkRollup>(),
            new List<string>());
    }

    public static IDefaultBenchmarkRollupStore NewCachedDefaultBenchmarkRollupStore(MemoryDefaultBenchmarkRollupStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultBenchmarkRollupStore(
            inner,
            DefaultBenchmarkRollupCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<DefaultBenchmarkRollup> FindDefaultBenchmarkRollups(IDefaultBenchmarkRollupStore store, DefaultBenchmarkRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
