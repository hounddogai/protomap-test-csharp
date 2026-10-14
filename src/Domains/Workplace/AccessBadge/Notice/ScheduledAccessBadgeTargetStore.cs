namespace Acme.Workforce.Workplace.AccessBadge.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled access badge targets by code.</summary>
public interface IScheduledAccessBadgeTargetStore
{
    ScheduledAccessBadgeTarget? Find(string code);
    void Save(ScheduledAccessBadgeTarget item);
    bool Remove(string code);
    List<ScheduledAccessBadgeTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledAccessBadgeTargetStore(Dictionary<string, ScheduledAccessBadgeTarget> items, List<string> order) : IScheduledAccessBadgeTargetStore
{
    public Dictionary<string, ScheduledAccessBadgeTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledAccessBadgeTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledAccessBadgeTarget item)
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

    public List<ScheduledAccessBadgeTarget> List()
    {
        var outValue = new List<ScheduledAccessBadgeTarget>();
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
public sealed class CachedScheduledAccessBadgeTargetStore(MemoryScheduledAccessBadgeTargetStore inner, ScheduledAccessBadgeTargetCache cache, int hits, int misses) : IScheduledAccessBadgeTargetStore
{
    public MemoryScheduledAccessBadgeTargetStore Inner { get; set; } = inner;
    public ScheduledAccessBadgeTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledAccessBadgeTarget? Find(string code)
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

    public void Save(ScheduledAccessBadgeTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledAccessBadgeTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled access badge targets. Empty fields match all.</summary>
public sealed record ScheduledAccessBadgeTargetQuery(string Name, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledAccessBadgeTarget item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledAccessBadgeTarget> Apply(List<ScheduledAccessBadgeTarget> items)
    {
        var matched = Collections.Filter(items, (ScheduledAccessBadgeTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled access badge target store.</summary>
public static class ScheduledAccessBadgeTargetStore
{
    public static IScheduledAccessBadgeTargetStore NewMemoryScheduledAccessBadgeTargetStore()
    {
        return new MemoryScheduledAccessBadgeTargetStore(
            new Dictionary<string, ScheduledAccessBadgeTarget>(),
            new List<string>());
    }

    public static IScheduledAccessBadgeTargetStore NewCachedScheduledAccessBadgeTargetStore(MemoryScheduledAccessBadgeTargetStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledAccessBadgeTargetStore(
            inner,
            ScheduledAccessBadgeTargetCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledAccessBadgeTarget> FindScheduledAccessBadgeTargets(IScheduledAccessBadgeTargetStore store, ScheduledAccessBadgeTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
