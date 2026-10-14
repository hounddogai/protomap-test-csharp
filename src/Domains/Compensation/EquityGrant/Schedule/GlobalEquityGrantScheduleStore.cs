namespace Acme.Workforce.Compensation.EquityGrant.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global equity grant schedules by code.</summary>
public interface IGlobalEquityGrantScheduleStore
{
    GlobalEquityGrantSchedule? Find(string code);
    void Save(GlobalEquityGrantSchedule item);
    bool Remove(string code);
    List<GlobalEquityGrantSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalEquityGrantScheduleStore(Dictionary<string, GlobalEquityGrantSchedule> items, List<string> order) : IGlobalEquityGrantScheduleStore
{
    public Dictionary<string, GlobalEquityGrantSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalEquityGrantSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalEquityGrantSchedule item)
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

    public List<GlobalEquityGrantSchedule> List()
    {
        var outValue = new List<GlobalEquityGrantSchedule>();
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
public sealed class CachedGlobalEquityGrantScheduleStore(MemoryGlobalEquityGrantScheduleStore inner, GlobalEquityGrantScheduleCache cache, int hits, int misses) : IGlobalEquityGrantScheduleStore
{
    public MemoryGlobalEquityGrantScheduleStore Inner { get; set; } = inner;
    public GlobalEquityGrantScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalEquityGrantSchedule? Find(string code)
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

    public void Save(GlobalEquityGrantSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalEquityGrantSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global equity grant schedules. Empty fields match all.</summary>
public sealed record GlobalEquityGrantScheduleQuery(string Name, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalEquityGrantSchedule item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<GlobalEquityGrantSchedule> Apply(List<GlobalEquityGrantSchedule> items)
    {
        var matched = Collections.Filter(items, (GlobalEquityGrantSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global equity grant schedule store.</summary>
public static class GlobalEquityGrantScheduleStore
{
    public static IGlobalEquityGrantScheduleStore NewMemoryGlobalEquityGrantScheduleStore()
    {
        return new MemoryGlobalEquityGrantScheduleStore(
            new Dictionary<string, GlobalEquityGrantSchedule>(),
            new List<string>());
    }

    public static IGlobalEquityGrantScheduleStore NewCachedGlobalEquityGrantScheduleStore(MemoryGlobalEquityGrantScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalEquityGrantScheduleStore(
            inner,
            GlobalEquityGrantScheduleCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<GlobalEquityGrantSchedule> FindGlobalEquityGrantSchedules(IGlobalEquityGrantScheduleStore store, GlobalEquityGrantScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
