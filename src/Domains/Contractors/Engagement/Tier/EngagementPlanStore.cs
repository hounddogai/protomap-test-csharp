namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps engagement plans by code.</summary>
public interface IEngagementPlanStore
{
    EngagementPlan? Find(string code);
    void Save(EngagementPlan item);
    bool Remove(string code);
    List<EngagementPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEngagementPlanStore(Dictionary<string, EngagementPlan> items, List<string> order) : IEngagementPlanStore
{
    public Dictionary<string, EngagementPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EngagementPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EngagementPlan item)
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

    public List<EngagementPlan> List()
    {
        var outValue = new List<EngagementPlan>();
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
public sealed class CachedEngagementPlanStore(MemoryEngagementPlanStore inner, EngagementPlanCache cache, int hits, int misses) : IEngagementPlanStore
{
    public MemoryEngagementPlanStore Inner { get; set; } = inner;
    public EngagementPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EngagementPlan? Find(string code)
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

    public void Save(EngagementPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EngagementPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects engagement plans. Empty fields match all.</summary>
public sealed record EngagementPlanQuery(string Segment, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(EngagementPlan item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EngagementPlan> Apply(List<EngagementPlan> items)
    {
        var matched = Collections.Filter(items, (EngagementPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of engagement plan store.</summary>
public static class EngagementPlanStore
{
    public static IEngagementPlanStore NewMemoryEngagementPlanStore()
    {
        return new MemoryEngagementPlanStore(new Dictionary<string, EngagementPlan>(), new List<string>());
    }

    public static IEngagementPlanStore NewCachedEngagementPlanStore(MemoryEngagementPlanStore inner, int ttl, IClock clock)
    {
        return new CachedEngagementPlanStore(inner, EngagementPlanCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<EngagementPlan> FindEngagementPlans(IEngagementPlanStore store, EngagementPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
