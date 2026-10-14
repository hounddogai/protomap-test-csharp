namespace Acme.Workforce.Talent.Mobility.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted mobility rollups by code.</summary>
public interface IWeightedMobilityRollupStore
{
    WeightedMobilityRollup? Find(string code);
    void Save(WeightedMobilityRollup item);
    bool Remove(string code);
    List<WeightedMobilityRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedMobilityRollupStore(Dictionary<string, WeightedMobilityRollup> items, List<string> order) : IWeightedMobilityRollupStore
{
    public Dictionary<string, WeightedMobilityRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedMobilityRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedMobilityRollup item)
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

    public List<WeightedMobilityRollup> List()
    {
        var outValue = new List<WeightedMobilityRollup>();
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
public sealed class CachedWeightedMobilityRollupStore(MemoryWeightedMobilityRollupStore inner, WeightedMobilityRollupCache cache, int hits, int misses) : IWeightedMobilityRollupStore
{
    public MemoryWeightedMobilityRollupStore Inner { get; set; } = inner;
    public WeightedMobilityRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedMobilityRollup? Find(string code)
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

    public void Save(WeightedMobilityRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedMobilityRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted mobility rollups. Empty fields match all.</summary>
public sealed record WeightedMobilityRollupQuery(string Name, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedMobilityRollup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedMobilityRollup> Apply(List<WeightedMobilityRollup> items)
    {
        var matched = Collections.Filter(items, (WeightedMobilityRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted mobility rollup store.</summary>
public static class WeightedMobilityRollupStore
{
    public static IWeightedMobilityRollupStore NewMemoryWeightedMobilityRollupStore()
    {
        return new MemoryWeightedMobilityRollupStore(
            new Dictionary<string, WeightedMobilityRollup>(),
            new List<string>());
    }

    public static IWeightedMobilityRollupStore NewCachedWeightedMobilityRollupStore(MemoryWeightedMobilityRollupStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedMobilityRollupStore(
            inner,
            WeightedMobilityRollupCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<WeightedMobilityRollup> FindWeightedMobilityRollups(IWeightedMobilityRollupStore store, WeightedMobilityRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
