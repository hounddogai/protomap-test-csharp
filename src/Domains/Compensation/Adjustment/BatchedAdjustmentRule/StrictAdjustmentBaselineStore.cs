namespace Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict adjustment baselines by code.</summary>
public interface IStrictAdjustmentBaselineStore
{
    StrictAdjustmentBaseline? Find(string code);
    void Save(StrictAdjustmentBaseline item);
    bool Remove(string code);
    List<StrictAdjustmentBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictAdjustmentBaselineStore(Dictionary<string, StrictAdjustmentBaseline> items, List<string> order) : IStrictAdjustmentBaselineStore
{
    public Dictionary<string, StrictAdjustmentBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictAdjustmentBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictAdjustmentBaseline item)
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

    public List<StrictAdjustmentBaseline> List()
    {
        var outValue = new List<StrictAdjustmentBaseline>();
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
public sealed class CachedStrictAdjustmentBaselineStore(MemoryStrictAdjustmentBaselineStore inner, StrictAdjustmentBaselineCache cache, int hits, int misses) : IStrictAdjustmentBaselineStore
{
    public MemoryStrictAdjustmentBaselineStore Inner { get; set; } = inner;
    public StrictAdjustmentBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictAdjustmentBaseline? Find(string code)
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

    public void Save(StrictAdjustmentBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictAdjustmentBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict adjustment baselines. Empty fields match all.</summary>
public sealed record StrictAdjustmentBaselineQuery(string Name, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(StrictAdjustmentBaseline item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<StrictAdjustmentBaseline> Apply(List<StrictAdjustmentBaseline> items)
    {
        var matched = Collections.Filter(items, (StrictAdjustmentBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict adjustment baseline store.</summary>
public static class StrictAdjustmentBaselineStore
{
    public static IStrictAdjustmentBaselineStore NewMemoryStrictAdjustmentBaselineStore()
    {
        return new MemoryStrictAdjustmentBaselineStore(
            new Dictionary<string, StrictAdjustmentBaseline>(),
            new List<string>());
    }

    public static IStrictAdjustmentBaselineStore NewCachedStrictAdjustmentBaselineStore(MemoryStrictAdjustmentBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedStrictAdjustmentBaselineStore(
            inner,
            StrictAdjustmentBaselineCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<StrictAdjustmentBaseline> FindStrictAdjustmentBaselines(IStrictAdjustmentBaselineStore store, StrictAdjustmentBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
