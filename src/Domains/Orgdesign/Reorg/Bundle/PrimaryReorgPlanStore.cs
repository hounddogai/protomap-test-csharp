namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary reorg plans by code.</summary>
public interface IPrimaryReorgPlanStore
{
    PrimaryReorgPlan? Find(string code);
    void Save(PrimaryReorgPlan item);
    bool Remove(string code);
    List<PrimaryReorgPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryReorgPlanStore(Dictionary<string, PrimaryReorgPlan> items, List<string> order) : IPrimaryReorgPlanStore
{
    public Dictionary<string, PrimaryReorgPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryReorgPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryReorgPlan item)
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

    public List<PrimaryReorgPlan> List()
    {
        var outValue = new List<PrimaryReorgPlan>();
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
public sealed class CachedPrimaryReorgPlanStore(MemoryPrimaryReorgPlanStore inner, PrimaryReorgPlanCache cache, int hits, int misses) : IPrimaryReorgPlanStore
{
    public MemoryPrimaryReorgPlanStore Inner { get; set; } = inner;
    public PrimaryReorgPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryReorgPlan? Find(string code)
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

    public void Save(PrimaryReorgPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryReorgPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary reorg plans. Empty fields match all.</summary>
public sealed record PrimaryReorgPlanQuery(string Label, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryReorgPlan item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryReorgPlan> Apply(List<PrimaryReorgPlan> items)
    {
        var matched = Collections.Filter(items, (PrimaryReorgPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary reorg plan store.</summary>
public static class PrimaryReorgPlanStore
{
    public static IPrimaryReorgPlanStore NewMemoryPrimaryReorgPlanStore()
    {
        return new MemoryPrimaryReorgPlanStore(new Dictionary<string, PrimaryReorgPlan>(), new List<string>());
    }

    public static IPrimaryReorgPlanStore NewCachedPrimaryReorgPlanStore(MemoryPrimaryReorgPlanStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryReorgPlanStore(inner, PrimaryReorgPlanCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PrimaryReorgPlan> FindPrimaryReorgPlans(IPrimaryReorgPlanStore store, PrimaryReorgPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
