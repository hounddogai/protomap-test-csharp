namespace Acme.Workforce.Compensation.PayBand.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted pay band estimates by code.</summary>
public interface IWeightedPayBandEstimateStore
{
    WeightedPayBandEstimate? Find(string code);
    void Save(WeightedPayBandEstimate item);
    bool Remove(string code);
    List<WeightedPayBandEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedPayBandEstimateStore(Dictionary<string, WeightedPayBandEstimate> items, List<string> order) : IWeightedPayBandEstimateStore
{
    public Dictionary<string, WeightedPayBandEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedPayBandEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedPayBandEstimate item)
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

    public List<WeightedPayBandEstimate> List()
    {
        var outValue = new List<WeightedPayBandEstimate>();
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
public sealed class CachedWeightedPayBandEstimateStore(MemoryWeightedPayBandEstimateStore inner, WeightedPayBandEstimateCache cache, int hits, int misses) : IWeightedPayBandEstimateStore
{
    public MemoryWeightedPayBandEstimateStore Inner { get; set; } = inner;
    public WeightedPayBandEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedPayBandEstimate? Find(string code)
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

    public void Save(WeightedPayBandEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedPayBandEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted pay band estimates. Empty fields match all.</summary>
public sealed record WeightedPayBandEstimateQuery(string Team, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedPayBandEstimate item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedPayBandEstimate> Apply(List<WeightedPayBandEstimate> items)
    {
        var matched = Collections.Filter(items, (WeightedPayBandEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted pay band estimate store.</summary>
public static class WeightedPayBandEstimateStore
{
    public static IWeightedPayBandEstimateStore NewMemoryWeightedPayBandEstimateStore()
    {
        return new MemoryWeightedPayBandEstimateStore(
            new Dictionary<string, WeightedPayBandEstimate>(),
            new List<string>());
    }

    public static IWeightedPayBandEstimateStore NewCachedWeightedPayBandEstimateStore(MemoryWeightedPayBandEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedPayBandEstimateStore(
            inner,
            WeightedPayBandEstimateCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<WeightedPayBandEstimate> FindWeightedPayBandEstimates(IWeightedPayBandEstimateStore store, WeightedPayBandEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
