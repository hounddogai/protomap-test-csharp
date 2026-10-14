namespace Acme.Workforce.Assets.Laptop.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted laptop estimates by code.</summary>
public interface IWeightedLaptopEstimateStore
{
    WeightedLaptopEstimate? Find(string code);
    void Save(WeightedLaptopEstimate item);
    bool Remove(string code);
    List<WeightedLaptopEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedLaptopEstimateStore(Dictionary<string, WeightedLaptopEstimate> items, List<string> order) : IWeightedLaptopEstimateStore
{
    public Dictionary<string, WeightedLaptopEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedLaptopEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedLaptopEstimate item)
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

    public List<WeightedLaptopEstimate> List()
    {
        var outValue = new List<WeightedLaptopEstimate>();
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
public sealed class CachedWeightedLaptopEstimateStore(MemoryWeightedLaptopEstimateStore inner, WeightedLaptopEstimateCache cache, int hits, int misses) : IWeightedLaptopEstimateStore
{
    public MemoryWeightedLaptopEstimateStore Inner { get; set; } = inner;
    public WeightedLaptopEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedLaptopEstimate? Find(string code)
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

    public void Save(WeightedLaptopEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedLaptopEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted laptop estimates. Empty fields match all.</summary>
public sealed record WeightedLaptopEstimateQuery(string Unit, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedLaptopEstimate item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedLaptopEstimate> Apply(List<WeightedLaptopEstimate> items)
    {
        var matched = Collections.Filter(items, (WeightedLaptopEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted laptop estimate store.</summary>
public static class WeightedLaptopEstimateStore
{
    public static IWeightedLaptopEstimateStore NewMemoryWeightedLaptopEstimateStore()
    {
        return new MemoryWeightedLaptopEstimateStore(
            new Dictionary<string, WeightedLaptopEstimate>(),
            new List<string>());
    }

    public static IWeightedLaptopEstimateStore NewCachedWeightedLaptopEstimateStore(MemoryWeightedLaptopEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedLaptopEstimateStore(
            inner,
            WeightedLaptopEstimateCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<WeightedLaptopEstimate> FindWeightedLaptopEstimates(IWeightedLaptopEstimateStore store, WeightedLaptopEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
