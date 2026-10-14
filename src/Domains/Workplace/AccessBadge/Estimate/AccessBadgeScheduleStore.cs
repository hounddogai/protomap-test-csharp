namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge schedules by code.</summary>
public interface IAccessBadgeScheduleStore
{
    AccessBadgeSchedule? Find(string code);
    void Save(AccessBadgeSchedule item);
    bool Remove(string code);
    List<AccessBadgeSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeScheduleStore(Dictionary<string, AccessBadgeSchedule> items, List<string> order) : IAccessBadgeScheduleStore
{
    public Dictionary<string, AccessBadgeSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeSchedule item)
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

    public List<AccessBadgeSchedule> List()
    {
        var outValue = new List<AccessBadgeSchedule>();
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
public sealed class CachedAccessBadgeScheduleStore(MemoryAccessBadgeScheduleStore inner, AccessBadgeScheduleCache cache, int hits, int misses) : IAccessBadgeScheduleStore
{
    public MemoryAccessBadgeScheduleStore Inner { get; set; } = inner;
    public AccessBadgeScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeSchedule? Find(string code)
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

    public void Save(AccessBadgeSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge schedules. Empty fields match all.</summary>
public sealed record AccessBadgeScheduleQuery(string Unit, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeSchedule item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<AccessBadgeSchedule> Apply(List<AccessBadgeSchedule> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge schedule store.</summary>
public static class AccessBadgeScheduleStore
{
    public static IAccessBadgeScheduleStore NewMemoryAccessBadgeScheduleStore()
    {
        return new MemoryAccessBadgeScheduleStore(new Dictionary<string, AccessBadgeSchedule>(), new List<string>());
    }

    public static IAccessBadgeScheduleStore NewCachedAccessBadgeScheduleStore(MemoryAccessBadgeScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeScheduleStore(inner, AccessBadgeScheduleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeSchedule> FindAccessBadgeSchedules(IAccessBadgeScheduleStore store, AccessBadgeScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
