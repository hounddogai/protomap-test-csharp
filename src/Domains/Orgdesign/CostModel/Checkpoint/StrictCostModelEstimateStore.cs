namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict cost model estimates by code.</summary>
public interface IStrictCostModelEstimateStore
{
    StrictCostModelEstimate? Find(string code);
    void Save(StrictCostModelEstimate item);
    bool Remove(string code);
    List<StrictCostModelEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictCostModelEstimateStore(Dictionary<string, StrictCostModelEstimate> items, List<string> order) : IStrictCostModelEstimateStore
{
    public Dictionary<string, StrictCostModelEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictCostModelEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictCostModelEstimate item)
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

    public List<StrictCostModelEstimate> List()
    {
        var outValue = new List<StrictCostModelEstimate>();
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
public sealed class CachedStrictCostModelEstimateStore(MemoryStrictCostModelEstimateStore inner, StrictCostModelEstimateCache cache, int hits, int misses) : IStrictCostModelEstimateStore
{
    public MemoryStrictCostModelEstimateStore Inner { get; set; } = inner;
    public StrictCostModelEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictCostModelEstimate? Find(string code)
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

    public void Save(StrictCostModelEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictCostModelEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict cost model estimates. Empty fields match all.</summary>
public sealed record StrictCostModelEstimateQuery(string Title, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(StrictCostModelEstimate item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictCostModelEstimate> Apply(List<StrictCostModelEstimate> items)
    {
        var matched = Collections.Filter(items, (StrictCostModelEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict cost model estimate store.</summary>
public static class StrictCostModelEstimateStore
{
    public static IStrictCostModelEstimateStore NewMemoryStrictCostModelEstimateStore()
    {
        return new MemoryStrictCostModelEstimateStore(
            new Dictionary<string, StrictCostModelEstimate>(),
            new List<string>());
    }

    public static IStrictCostModelEstimateStore NewCachedStrictCostModelEstimateStore(MemoryStrictCostModelEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedStrictCostModelEstimateStore(
            inner,
            StrictCostModelEstimateCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<StrictCostModelEstimate> FindStrictCostModelEstimates(IStrictCostModelEstimateStore store, StrictCostModelEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
