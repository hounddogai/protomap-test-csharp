namespace Acme.Workforce.Engagement.ActionPlan.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan summaries by code.</summary>
public interface IActionPlanSummaryStore
{
    ActionPlanSummary? Find(string code);
    void Save(ActionPlanSummary item);
    bool Remove(string code);
    List<ActionPlanSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanSummaryStore(Dictionary<string, ActionPlanSummary> items, List<string> order) : IActionPlanSummaryStore
{
    public Dictionary<string, ActionPlanSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanSummary item)
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

    public List<ActionPlanSummary> List()
    {
        var outValue = new List<ActionPlanSummary>();
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
public sealed class CachedActionPlanSummaryStore(MemoryActionPlanSummaryStore inner, ActionPlanSummaryCache cache, int hits, int misses) : IActionPlanSummaryStore
{
    public MemoryActionPlanSummaryStore Inner { get; set; } = inner;
    public ActionPlanSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanSummary? Find(string code)
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

    public void Save(ActionPlanSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan summaries. Empty fields match all.</summary>
public sealed record ActionPlanSummaryQuery(string Name, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanSummary item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ActionPlanSummary> Apply(List<ActionPlanSummary> items)
    {
        var matched = Collections.Filter(items, (ActionPlanSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan summary store.</summary>
public static class ActionPlanSummaryStore
{
    public static IActionPlanSummaryStore NewMemoryActionPlanSummaryStore()
    {
        return new MemoryActionPlanSummaryStore(new Dictionary<string, ActionPlanSummary>(), new List<string>());
    }

    public static IActionPlanSummaryStore NewCachedActionPlanSummaryStore(MemoryActionPlanSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanSummaryStore(inner, ActionPlanSummaryCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ActionPlanSummary> FindActionPlanSummaries(IActionPlanSummaryStore store, ActionPlanSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
