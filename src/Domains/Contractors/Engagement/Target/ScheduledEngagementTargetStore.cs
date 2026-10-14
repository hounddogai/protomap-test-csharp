namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled engagement targets by code.</summary>
public interface IScheduledEngagementTargetStore
{
    ScheduledEngagementTarget? Find(string code);
    void Save(ScheduledEngagementTarget item);
    bool Remove(string code);
    List<ScheduledEngagementTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledEngagementTargetStore(Dictionary<string, ScheduledEngagementTarget> items, List<string> order) : IScheduledEngagementTargetStore
{
    public Dictionary<string, ScheduledEngagementTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledEngagementTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledEngagementTarget item)
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

    public List<ScheduledEngagementTarget> List()
    {
        var outValue = new List<ScheduledEngagementTarget>();
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
public sealed class CachedScheduledEngagementTargetStore(MemoryScheduledEngagementTargetStore inner, ScheduledEngagementTargetCache cache, int hits, int misses) : IScheduledEngagementTargetStore
{
    public MemoryScheduledEngagementTargetStore Inner { get; set; } = inner;
    public ScheduledEngagementTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledEngagementTarget? Find(string code)
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

    public void Save(ScheduledEngagementTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledEngagementTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled engagement targets. Empty fields match all.</summary>
public sealed record ScheduledEngagementTargetQuery(string Category, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledEngagementTarget item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<ScheduledEngagementTarget> Apply(List<ScheduledEngagementTarget> items)
    {
        var matched = Collections.Filter(items, (ScheduledEngagementTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled engagement target store.</summary>
public static class ScheduledEngagementTargetStore
{
    public static IScheduledEngagementTargetStore NewMemoryScheduledEngagementTargetStore()
    {
        return new MemoryScheduledEngagementTargetStore(
            new Dictionary<string, ScheduledEngagementTarget>(),
            new List<string>());
    }

    public static IScheduledEngagementTargetStore NewCachedScheduledEngagementTargetStore(MemoryScheduledEngagementTargetStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledEngagementTargetStore(
            inner,
            ScheduledEngagementTargetCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledEngagementTarget> FindScheduledEngagementTargets(IScheduledEngagementTargetStore store, ScheduledEngagementTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
