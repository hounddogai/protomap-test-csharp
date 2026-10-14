namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor plans by code.</summary>
public interface IVisitorPlanStore
{
    VisitorPlan? Find(string code);
    void Save(VisitorPlan item);
    bool Remove(string code);
    List<VisitorPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorPlanStore(Dictionary<string, VisitorPlan> items, List<string> order) : IVisitorPlanStore
{
    public Dictionary<string, VisitorPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorPlan item)
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

    public List<VisitorPlan> List()
    {
        var outValue = new List<VisitorPlan>();
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
public sealed class CachedVisitorPlanStore(MemoryVisitorPlanStore inner, VisitorPlanCache cache, int hits, int misses) : IVisitorPlanStore
{
    public MemoryVisitorPlanStore Inner { get; set; } = inner;
    public VisitorPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorPlan? Find(string code)
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

    public void Save(VisitorPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor plans. Empty fields match all.</summary>
public sealed record VisitorPlanQuery(string Source, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorPlan item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorPlan> Apply(List<VisitorPlan> items)
    {
        var matched = Collections.Filter(items, (VisitorPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor plan store.</summary>
public static class VisitorPlanStore
{
    public static IVisitorPlanStore NewMemoryVisitorPlanStore()
    {
        return new MemoryVisitorPlanStore(new Dictionary<string, VisitorPlan>(), new List<string>());
    }

    public static IVisitorPlanStore NewCachedVisitorPlanStore(MemoryVisitorPlanStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorPlanStore(inner, VisitorPlanCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<VisitorPlan> FindVisitorPlans(IVisitorPlanStore store, VisitorPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
