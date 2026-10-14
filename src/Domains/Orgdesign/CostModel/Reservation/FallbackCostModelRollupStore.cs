namespace Acme.Workforce.Orgdesign.CostModel.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback cost model rollups by code.</summary>
public interface IFallbackCostModelRollupStore
{
    FallbackCostModelRollup? Find(string code);
    void Save(FallbackCostModelRollup item);
    bool Remove(string code);
    List<FallbackCostModelRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackCostModelRollupStore(Dictionary<string, FallbackCostModelRollup> items, List<string> order) : IFallbackCostModelRollupStore
{
    public Dictionary<string, FallbackCostModelRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackCostModelRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackCostModelRollup item)
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

    public List<FallbackCostModelRollup> List()
    {
        var outValue = new List<FallbackCostModelRollup>();
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
public sealed class CachedFallbackCostModelRollupStore(MemoryFallbackCostModelRollupStore inner, FallbackCostModelRollupCache cache, int hits, int misses) : IFallbackCostModelRollupStore
{
    public MemoryFallbackCostModelRollupStore Inner { get; set; } = inner;
    public FallbackCostModelRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackCostModelRollup? Find(string code)
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

    public void Save(FallbackCostModelRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackCostModelRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback cost model rollups. Empty fields match all.</summary>
public sealed record FallbackCostModelRollupQuery(string Source, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackCostModelRollup item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackCostModelRollup> Apply(List<FallbackCostModelRollup> items)
    {
        var matched = Collections.Filter(items, (FallbackCostModelRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback cost model rollup store.</summary>
public static class FallbackCostModelRollupStore
{
    public static IFallbackCostModelRollupStore NewMemoryFallbackCostModelRollupStore()
    {
        return new MemoryFallbackCostModelRollupStore(
            new Dictionary<string, FallbackCostModelRollup>(),
            new List<string>());
    }

    public static IFallbackCostModelRollupStore NewCachedFallbackCostModelRollupStore(MemoryFallbackCostModelRollupStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackCostModelRollupStore(
            inner,
            FallbackCostModelRollupCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<FallbackCostModelRollup> FindFallbackCostModelRollups(IFallbackCostModelRollupStore store, FallbackCostModelRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
