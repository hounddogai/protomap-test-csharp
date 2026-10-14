namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict return plan rollups by code.</summary>
public interface IStrictReturnPlanRollupStore
{
    StrictReturnPlanRollup? Find(string code);
    void Save(StrictReturnPlanRollup item);
    bool Remove(string code);
    List<StrictReturnPlanRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictReturnPlanRollupStore(Dictionary<string, StrictReturnPlanRollup> items, List<string> order) : IStrictReturnPlanRollupStore
{
    public Dictionary<string, StrictReturnPlanRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictReturnPlanRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictReturnPlanRollup item)
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

    public List<StrictReturnPlanRollup> List()
    {
        var outValue = new List<StrictReturnPlanRollup>();
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
public sealed class CachedStrictReturnPlanRollupStore(MemoryStrictReturnPlanRollupStore inner, StrictReturnPlanRollupCache cache, int hits, int misses) : IStrictReturnPlanRollupStore
{
    public MemoryStrictReturnPlanRollupStore Inner { get; set; } = inner;
    public StrictReturnPlanRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictReturnPlanRollup? Find(string code)
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

    public void Save(StrictReturnPlanRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictReturnPlanRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict return plan rollups. Empty fields match all.</summary>
public sealed record StrictReturnPlanRollupQuery(string Source, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(StrictReturnPlanRollup item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictReturnPlanRollup> Apply(List<StrictReturnPlanRollup> items)
    {
        var matched = Collections.Filter(items, (StrictReturnPlanRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict return plan rollup store.</summary>
public static class StrictReturnPlanRollupStore
{
    public static IStrictReturnPlanRollupStore NewMemoryStrictReturnPlanRollupStore()
    {
        return new MemoryStrictReturnPlanRollupStore(
            new Dictionary<string, StrictReturnPlanRollup>(),
            new List<string>());
    }

    public static IStrictReturnPlanRollupStore NewCachedStrictReturnPlanRollupStore(MemoryStrictReturnPlanRollupStore inner, int ttl, IClock clock)
    {
        return new CachedStrictReturnPlanRollupStore(
            inner,
            StrictReturnPlanRollupCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<StrictReturnPlanRollup> FindStrictReturnPlanRollups(IStrictReturnPlanRollupStore store, StrictReturnPlanRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
