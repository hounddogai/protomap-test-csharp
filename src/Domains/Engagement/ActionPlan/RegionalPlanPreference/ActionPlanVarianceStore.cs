namespace Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan variances by code.</summary>
public interface IActionPlanVarianceStore
{
    ActionPlanVariance? Find(string code);
    void Save(ActionPlanVariance item);
    bool Remove(string code);
    List<ActionPlanVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanVarianceStore(Dictionary<string, ActionPlanVariance> items, List<string> order) : IActionPlanVarianceStore
{
    public Dictionary<string, ActionPlanVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanVariance item)
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

    public List<ActionPlanVariance> List()
    {
        var outValue = new List<ActionPlanVariance>();
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
public sealed class CachedActionPlanVarianceStore(MemoryActionPlanVarianceStore inner, ActionPlanVarianceCache cache, int hits, int misses) : IActionPlanVarianceStore
{
    public MemoryActionPlanVarianceStore Inner { get; set; } = inner;
    public ActionPlanVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanVariance? Find(string code)
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

    public void Save(ActionPlanVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan variances. Empty fields match all.</summary>
public sealed record ActionPlanVarianceQuery(string Label, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanVariance item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ActionPlanVariance> Apply(List<ActionPlanVariance> items)
    {
        var matched = Collections.Filter(items, (ActionPlanVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan variance store.</summary>
public static class ActionPlanVarianceStore
{
    public static IActionPlanVarianceStore NewMemoryActionPlanVarianceStore()
    {
        return new MemoryActionPlanVarianceStore(new Dictionary<string, ActionPlanVariance>(), new List<string>());
    }

    public static IActionPlanVarianceStore NewCachedActionPlanVarianceStore(MemoryActionPlanVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanVarianceStore(inner, ActionPlanVarianceCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ActionPlanVariance> FindActionPlanVariances(IActionPlanVarianceStore store, ActionPlanVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
