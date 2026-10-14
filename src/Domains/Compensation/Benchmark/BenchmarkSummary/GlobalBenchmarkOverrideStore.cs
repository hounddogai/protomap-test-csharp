namespace Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global benchmark overrides by code.</summary>
public interface IGlobalBenchmarkOverrideStore
{
    GlobalBenchmarkOverride? Find(string code);
    void Save(GlobalBenchmarkOverride item);
    bool Remove(string code);
    List<GlobalBenchmarkOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalBenchmarkOverrideStore(Dictionary<string, GlobalBenchmarkOverride> items, List<string> order) : IGlobalBenchmarkOverrideStore
{
    public Dictionary<string, GlobalBenchmarkOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalBenchmarkOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalBenchmarkOverride item)
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

    public List<GlobalBenchmarkOverride> List()
    {
        var outValue = new List<GlobalBenchmarkOverride>();
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
public sealed class CachedGlobalBenchmarkOverrideStore(MemoryGlobalBenchmarkOverrideStore inner, GlobalBenchmarkOverrideCache cache, int hits, int misses) : IGlobalBenchmarkOverrideStore
{
    public MemoryGlobalBenchmarkOverrideStore Inner { get; set; } = inner;
    public GlobalBenchmarkOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalBenchmarkOverride? Find(string code)
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

    public void Save(GlobalBenchmarkOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalBenchmarkOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global benchmark overrides. Empty fields match all.</summary>
public sealed record GlobalBenchmarkOverrideQuery(string Category, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalBenchmarkOverride item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalBenchmarkOverride> Apply(List<GlobalBenchmarkOverride> items)
    {
        var matched = Collections.Filter(items, (GlobalBenchmarkOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global benchmark override store.</summary>
public static class GlobalBenchmarkOverrideStore
{
    public static IGlobalBenchmarkOverrideStore NewMemoryGlobalBenchmarkOverrideStore()
    {
        return new MemoryGlobalBenchmarkOverrideStore(
            new Dictionary<string, GlobalBenchmarkOverride>(),
            new List<string>());
    }

    public static IGlobalBenchmarkOverrideStore NewCachedGlobalBenchmarkOverrideStore(MemoryGlobalBenchmarkOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalBenchmarkOverrideStore(
            inner,
            GlobalBenchmarkOverrideCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<GlobalBenchmarkOverride> FindGlobalBenchmarkOverrides(IGlobalBenchmarkOverrideStore store, GlobalBenchmarkOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
