namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback scenario overrides by code.</summary>
public interface IFallbackScenarioOverrideStore
{
    FallbackScenarioOverride? Find(string code);
    void Save(FallbackScenarioOverride item);
    bool Remove(string code);
    List<FallbackScenarioOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackScenarioOverrideStore(Dictionary<string, FallbackScenarioOverride> items, List<string> order) : IFallbackScenarioOverrideStore
{
    public Dictionary<string, FallbackScenarioOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackScenarioOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackScenarioOverride item)
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

    public List<FallbackScenarioOverride> List()
    {
        var outValue = new List<FallbackScenarioOverride>();
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
public sealed class CachedFallbackScenarioOverrideStore(MemoryFallbackScenarioOverrideStore inner, FallbackScenarioOverrideCache cache, int hits, int misses) : IFallbackScenarioOverrideStore
{
    public MemoryFallbackScenarioOverrideStore Inner { get; set; } = inner;
    public FallbackScenarioOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackScenarioOverride? Find(string code)
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

    public void Save(FallbackScenarioOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackScenarioOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback scenario overrides. Empty fields match all.</summary>
public sealed record FallbackScenarioOverrideQuery(string Region, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackScenarioOverride item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<FallbackScenarioOverride> Apply(List<FallbackScenarioOverride> items)
    {
        var matched = Collections.Filter(items, (FallbackScenarioOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback scenario override store.</summary>
public static class FallbackScenarioOverrideStore
{
    public static IFallbackScenarioOverrideStore NewMemoryFallbackScenarioOverrideStore()
    {
        return new MemoryFallbackScenarioOverrideStore(
            new Dictionary<string, FallbackScenarioOverride>(),
            new List<string>());
    }

    public static IFallbackScenarioOverrideStore NewCachedFallbackScenarioOverrideStore(MemoryFallbackScenarioOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackScenarioOverrideStore(
            inner,
            FallbackScenarioOverrideCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<FallbackScenarioOverride> FindFallbackScenarioOverrides(IFallbackScenarioOverrideStore store, FallbackScenarioOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
