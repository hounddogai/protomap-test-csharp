namespace Acme.Workforce.Orgdesign.Headcount.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted headcount rollups by code.</summary>
public interface IWeightedHeadcountRollupStore
{
    WeightedHeadcountRollup? Find(string code);
    void Save(WeightedHeadcountRollup item);
    bool Remove(string code);
    List<WeightedHeadcountRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedHeadcountRollupStore(Dictionary<string, WeightedHeadcountRollup> items, List<string> order) : IWeightedHeadcountRollupStore
{
    public Dictionary<string, WeightedHeadcountRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedHeadcountRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedHeadcountRollup item)
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

    public List<WeightedHeadcountRollup> List()
    {
        var outValue = new List<WeightedHeadcountRollup>();
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
public sealed class CachedWeightedHeadcountRollupStore(MemoryWeightedHeadcountRollupStore inner, WeightedHeadcountRollupCache cache, int hits, int misses) : IWeightedHeadcountRollupStore
{
    public MemoryWeightedHeadcountRollupStore Inner { get; set; } = inner;
    public WeightedHeadcountRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedHeadcountRollup? Find(string code)
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

    public void Save(WeightedHeadcountRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedHeadcountRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted headcount rollups. Empty fields match all.</summary>
public sealed record WeightedHeadcountRollupQuery(string Label, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedHeadcountRollup item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<WeightedHeadcountRollup> Apply(List<WeightedHeadcountRollup> items)
    {
        var matched = Collections.Filter(items, (WeightedHeadcountRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted headcount rollup store.</summary>
public static class WeightedHeadcountRollupStore
{
    public static IWeightedHeadcountRollupStore NewMemoryWeightedHeadcountRollupStore()
    {
        return new MemoryWeightedHeadcountRollupStore(
            new Dictionary<string, WeightedHeadcountRollup>(),
            new List<string>());
    }

    public static IWeightedHeadcountRollupStore NewCachedWeightedHeadcountRollupStore(MemoryWeightedHeadcountRollupStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedHeadcountRollupStore(
            inner,
            WeightedHeadcountRollupCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<WeightedHeadcountRollup> FindWeightedHeadcountRollups(IWeightedHeadcountRollupStore store, WeightedHeadcountRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
