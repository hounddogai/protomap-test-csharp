namespace Acme.Workforce.Engagement.ActionPlan.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan metrics by code.</summary>
public interface IActionPlanMetricStore
{
    ActionPlanMetric? Find(string code);
    void Save(ActionPlanMetric item);
    bool Remove(string code);
    List<ActionPlanMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanMetricStore(Dictionary<string, ActionPlanMetric> items, List<string> order) : IActionPlanMetricStore
{
    public Dictionary<string, ActionPlanMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanMetric item)
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

    public List<ActionPlanMetric> List()
    {
        var outValue = new List<ActionPlanMetric>();
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
public sealed class CachedActionPlanMetricStore(MemoryActionPlanMetricStore inner, ActionPlanMetricCache cache, int hits, int misses) : IActionPlanMetricStore
{
    public MemoryActionPlanMetricStore Inner { get; set; } = inner;
    public ActionPlanMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanMetric? Find(string code)
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

    public void Save(ActionPlanMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan metrics. Empty fields match all.</summary>
public sealed record ActionPlanMetricQuery(string Title, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanMetric item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ActionPlanMetric> Apply(List<ActionPlanMetric> items)
    {
        var matched = Collections.Filter(items, (ActionPlanMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan metric store.</summary>
public static class ActionPlanMetricStore
{
    public static IActionPlanMetricStore NewMemoryActionPlanMetricStore()
    {
        return new MemoryActionPlanMetricStore(new Dictionary<string, ActionPlanMetric>(), new List<string>());
    }

    public static IActionPlanMetricStore NewCachedActionPlanMetricStore(MemoryActionPlanMetricStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanMetricStore(inner, ActionPlanMetricCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ActionPlanMetric> FindActionPlanMetrics(IActionPlanMetricStore store, ActionPlanMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
