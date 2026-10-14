namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled leave request rollups by code.</summary>
public interface IScheduledLeaveRequestRollupStore
{
    ScheduledLeaveRequestRollup? Find(string code);
    void Save(ScheduledLeaveRequestRollup item);
    bool Remove(string code);
    List<ScheduledLeaveRequestRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledLeaveRequestRollupStore(Dictionary<string, ScheduledLeaveRequestRollup> items, List<string> order) : IScheduledLeaveRequestRollupStore
{
    public Dictionary<string, ScheduledLeaveRequestRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledLeaveRequestRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledLeaveRequestRollup item)
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

    public List<ScheduledLeaveRequestRollup> List()
    {
        var outValue = new List<ScheduledLeaveRequestRollup>();
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
public sealed class CachedScheduledLeaveRequestRollupStore(MemoryScheduledLeaveRequestRollupStore inner, ScheduledLeaveRequestRollupCache cache, int hits, int misses) : IScheduledLeaveRequestRollupStore
{
    public MemoryScheduledLeaveRequestRollupStore Inner { get; set; } = inner;
    public ScheduledLeaveRequestRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledLeaveRequestRollup? Find(string code)
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

    public void Save(ScheduledLeaveRequestRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledLeaveRequestRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled leave request rollups. Empty fields match all.</summary>
public sealed record ScheduledLeaveRequestRollupQuery(string Channel, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledLeaveRequestRollup item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledLeaveRequestRollup> Apply(List<ScheduledLeaveRequestRollup> items)
    {
        var matched = Collections.Filter(items, (ScheduledLeaveRequestRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled leave request rollup store.</summary>
public static class ScheduledLeaveRequestRollupStore
{
    public static IScheduledLeaveRequestRollupStore NewMemoryScheduledLeaveRequestRollupStore()
    {
        return new MemoryScheduledLeaveRequestRollupStore(
            new Dictionary<string, ScheduledLeaveRequestRollup>(),
            new List<string>());
    }

    public static IScheduledLeaveRequestRollupStore NewCachedScheduledLeaveRequestRollupStore(MemoryScheduledLeaveRequestRollupStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledLeaveRequestRollupStore(
            inner,
            ScheduledLeaveRequestRollupCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledLeaveRequestRollup> FindScheduledLeaveRequestRollups(IScheduledLeaveRequestRollupStore store, ScheduledLeaveRequestRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
