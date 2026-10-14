namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict reorg estimates by code.</summary>
public interface IStrictReorgEstimateStore
{
    StrictReorgEstimate? Find(string code);
    void Save(StrictReorgEstimate item);
    bool Remove(string code);
    List<StrictReorgEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictReorgEstimateStore(Dictionary<string, StrictReorgEstimate> items, List<string> order) : IStrictReorgEstimateStore
{
    public Dictionary<string, StrictReorgEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictReorgEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictReorgEstimate item)
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

    public List<StrictReorgEstimate> List()
    {
        var outValue = new List<StrictReorgEstimate>();
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
public sealed class CachedStrictReorgEstimateStore(MemoryStrictReorgEstimateStore inner, StrictReorgEstimateCache cache, int hits, int misses) : IStrictReorgEstimateStore
{
    public MemoryStrictReorgEstimateStore Inner { get; set; } = inner;
    public StrictReorgEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictReorgEstimate? Find(string code)
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

    public void Save(StrictReorgEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictReorgEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict reorg estimates. Empty fields match all.</summary>
public sealed record StrictReorgEstimateQuery(string Name, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(StrictReorgEstimate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictReorgEstimate> Apply(List<StrictReorgEstimate> items)
    {
        var matched = Collections.Filter(items, (StrictReorgEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict reorg estimate store.</summary>
public static class StrictReorgEstimateStore
{
    public static IStrictReorgEstimateStore NewMemoryStrictReorgEstimateStore()
    {
        return new MemoryStrictReorgEstimateStore(new Dictionary<string, StrictReorgEstimate>(), new List<string>());
    }

    public static IStrictReorgEstimateStore NewCachedStrictReorgEstimateStore(MemoryStrictReorgEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedStrictReorgEstimateStore(inner, StrictReorgEstimateCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<StrictReorgEstimate> FindStrictReorgEstimates(IStrictReorgEstimateStore store, StrictReorgEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
