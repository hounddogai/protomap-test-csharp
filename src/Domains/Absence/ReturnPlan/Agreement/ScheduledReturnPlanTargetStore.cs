namespace Acme.Workforce.Absence.ReturnPlan.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled return plan targets by code.</summary>
public interface IScheduledReturnPlanTargetStore
{
    ScheduledReturnPlanTarget? Find(string code);
    void Save(ScheduledReturnPlanTarget item);
    bool Remove(string code);
    List<ScheduledReturnPlanTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledReturnPlanTargetStore(Dictionary<string, ScheduledReturnPlanTarget> items, List<string> order) : IScheduledReturnPlanTargetStore
{
    public Dictionary<string, ScheduledReturnPlanTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledReturnPlanTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledReturnPlanTarget item)
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

    public List<ScheduledReturnPlanTarget> List()
    {
        var outValue = new List<ScheduledReturnPlanTarget>();
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
public sealed class CachedScheduledReturnPlanTargetStore(MemoryScheduledReturnPlanTargetStore inner, ScheduledReturnPlanTargetCache cache, int hits, int misses) : IScheduledReturnPlanTargetStore
{
    public MemoryScheduledReturnPlanTargetStore Inner { get; set; } = inner;
    public ScheduledReturnPlanTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledReturnPlanTarget? Find(string code)
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

    public void Save(ScheduledReturnPlanTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledReturnPlanTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled return plan targets. Empty fields match all.</summary>
public sealed record ScheduledReturnPlanTargetQuery(string Channel, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledReturnPlanTarget item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledReturnPlanTarget> Apply(List<ScheduledReturnPlanTarget> items)
    {
        var matched = Collections.Filter(items, (ScheduledReturnPlanTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled return plan target store.</summary>
public static class ScheduledReturnPlanTargetStore
{
    public static IScheduledReturnPlanTargetStore NewMemoryScheduledReturnPlanTargetStore()
    {
        return new MemoryScheduledReturnPlanTargetStore(
            new Dictionary<string, ScheduledReturnPlanTarget>(),
            new List<string>());
    }

    public static IScheduledReturnPlanTargetStore NewCachedScheduledReturnPlanTargetStore(MemoryScheduledReturnPlanTargetStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledReturnPlanTargetStore(
            inner,
            ScheduledReturnPlanTargetCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledReturnPlanTarget> FindScheduledReturnPlanTargets(IScheduledReturnPlanTargetStore store, ScheduledReturnPlanTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
