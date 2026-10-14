namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled leave request overrides by code.</summary>
public interface IScheduledLeaveRequestOverrideStore
{
    ScheduledLeaveRequestOverride? Find(string code);
    void Save(ScheduledLeaveRequestOverride item);
    bool Remove(string code);
    List<ScheduledLeaveRequestOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledLeaveRequestOverrideStore(Dictionary<string, ScheduledLeaveRequestOverride> items, List<string> order) : IScheduledLeaveRequestOverrideStore
{
    public Dictionary<string, ScheduledLeaveRequestOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledLeaveRequestOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledLeaveRequestOverride item)
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

    public List<ScheduledLeaveRequestOverride> List()
    {
        var outValue = new List<ScheduledLeaveRequestOverride>();
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
public sealed class CachedScheduledLeaveRequestOverrideStore(MemoryScheduledLeaveRequestOverrideStore inner, ScheduledLeaveRequestOverrideCache cache, int hits, int misses) : IScheduledLeaveRequestOverrideStore
{
    public MemoryScheduledLeaveRequestOverrideStore Inner { get; set; } = inner;
    public ScheduledLeaveRequestOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledLeaveRequestOverride? Find(string code)
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

    public void Save(ScheduledLeaveRequestOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledLeaveRequestOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled leave request overrides. Empty fields match all.</summary>
public sealed record ScheduledLeaveRequestOverrideQuery(string Locale, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledLeaveRequestOverride item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledLeaveRequestOverride> Apply(List<ScheduledLeaveRequestOverride> items)
    {
        var matched = Collections.Filter(items, (ScheduledLeaveRequestOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled leave request override store.</summary>
public static class ScheduledLeaveRequestOverrideStore
{
    public static IScheduledLeaveRequestOverrideStore NewMemoryScheduledLeaveRequestOverrideStore()
    {
        return new MemoryScheduledLeaveRequestOverrideStore(
            new Dictionary<string, ScheduledLeaveRequestOverride>(),
            new List<string>());
    }

    public static IScheduledLeaveRequestOverrideStore NewCachedScheduledLeaveRequestOverrideStore(MemoryScheduledLeaveRequestOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledLeaveRequestOverrideStore(
            inner,
            ScheduledLeaveRequestOverrideCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledLeaveRequestOverride> FindScheduledLeaveRequestOverrides(IScheduledLeaveRequestOverrideStore store, ScheduledLeaveRequestOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
