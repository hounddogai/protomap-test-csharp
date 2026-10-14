namespace Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled leave request baselines by code.</summary>
public interface IScheduledLeaveRequestBaselineStore
{
    ScheduledLeaveRequestBaseline? Find(string code);
    void Save(ScheduledLeaveRequestBaseline item);
    bool Remove(string code);
    List<ScheduledLeaveRequestBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledLeaveRequestBaselineStore(Dictionary<string, ScheduledLeaveRequestBaseline> items, List<string> order) : IScheduledLeaveRequestBaselineStore
{
    public Dictionary<string, ScheduledLeaveRequestBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledLeaveRequestBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledLeaveRequestBaseline item)
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

    public List<ScheduledLeaveRequestBaseline> List()
    {
        var outValue = new List<ScheduledLeaveRequestBaseline>();
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
public sealed class CachedScheduledLeaveRequestBaselineStore(MemoryScheduledLeaveRequestBaselineStore inner, ScheduledLeaveRequestBaselineCache cache, int hits, int misses) : IScheduledLeaveRequestBaselineStore
{
    public MemoryScheduledLeaveRequestBaselineStore Inner { get; set; } = inner;
    public ScheduledLeaveRequestBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledLeaveRequestBaseline? Find(string code)
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

    public void Save(ScheduledLeaveRequestBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledLeaveRequestBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled leave request baselines. Empty fields match all.</summary>
public sealed record ScheduledLeaveRequestBaselineQuery(string Region, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledLeaveRequestBaseline item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledLeaveRequestBaseline> Apply(List<ScheduledLeaveRequestBaseline> items)
    {
        var matched = Collections.Filter(items, (ScheduledLeaveRequestBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled leave request baseline store.</summary>
public static class ScheduledLeaveRequestBaselineStore
{
    public static IScheduledLeaveRequestBaselineStore NewMemoryScheduledLeaveRequestBaselineStore()
    {
        return new MemoryScheduledLeaveRequestBaselineStore(
            new Dictionary<string, ScheduledLeaveRequestBaseline>(),
            new List<string>());
    }

    public static IScheduledLeaveRequestBaselineStore NewCachedScheduledLeaveRequestBaselineStore(MemoryScheduledLeaveRequestBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledLeaveRequestBaselineStore(
            inner,
            ScheduledLeaveRequestBaselineCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledLeaveRequestBaseline> FindScheduledLeaveRequestBaselines(IScheduledLeaveRequestBaselineStore store, ScheduledLeaveRequestBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
