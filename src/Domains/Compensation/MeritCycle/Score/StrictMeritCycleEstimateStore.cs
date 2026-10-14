namespace Acme.Workforce.Compensation.MeritCycle.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict merit cycle estimates by code.</summary>
public interface IStrictMeritCycleEstimateStore
{
    StrictMeritCycleEstimate? Find(string code);
    void Save(StrictMeritCycleEstimate item);
    bool Remove(string code);
    List<StrictMeritCycleEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictMeritCycleEstimateStore(Dictionary<string, StrictMeritCycleEstimate> items, List<string> order) : IStrictMeritCycleEstimateStore
{
    public Dictionary<string, StrictMeritCycleEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictMeritCycleEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictMeritCycleEstimate item)
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

    public List<StrictMeritCycleEstimate> List()
    {
        var outValue = new List<StrictMeritCycleEstimate>();
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
public sealed class CachedStrictMeritCycleEstimateStore(MemoryStrictMeritCycleEstimateStore inner, StrictMeritCycleEstimateCache cache, int hits, int misses) : IStrictMeritCycleEstimateStore
{
    public MemoryStrictMeritCycleEstimateStore Inner { get; set; } = inner;
    public StrictMeritCycleEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictMeritCycleEstimate? Find(string code)
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

    public void Save(StrictMeritCycleEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictMeritCycleEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict merit cycle estimates. Empty fields match all.</summary>
public sealed record StrictMeritCycleEstimateQuery(string Label, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(StrictMeritCycleEstimate item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictMeritCycleEstimate> Apply(List<StrictMeritCycleEstimate> items)
    {
        var matched = Collections.Filter(items, (StrictMeritCycleEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict merit cycle estimate store.</summary>
public static class StrictMeritCycleEstimateStore
{
    public static IStrictMeritCycleEstimateStore NewMemoryStrictMeritCycleEstimateStore()
    {
        return new MemoryStrictMeritCycleEstimateStore(
            new Dictionary<string, StrictMeritCycleEstimate>(),
            new List<string>());
    }

    public static IStrictMeritCycleEstimateStore NewCachedStrictMeritCycleEstimateStore(MemoryStrictMeritCycleEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedStrictMeritCycleEstimateStore(
            inner,
            StrictMeritCycleEstimateCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<StrictMeritCycleEstimate> FindStrictMeritCycleEstimates(IStrictMeritCycleEstimateStore store, StrictMeritCycleEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
