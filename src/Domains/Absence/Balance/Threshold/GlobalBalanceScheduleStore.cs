namespace Acme.Workforce.Absence.Balance.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global balance schedules by code.</summary>
public interface IGlobalBalanceScheduleStore
{
    GlobalBalanceSchedule? Find(string code);
    void Save(GlobalBalanceSchedule item);
    bool Remove(string code);
    List<GlobalBalanceSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalBalanceScheduleStore(Dictionary<string, GlobalBalanceSchedule> items, List<string> order) : IGlobalBalanceScheduleStore
{
    public Dictionary<string, GlobalBalanceSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalBalanceSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalBalanceSchedule item)
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

    public List<GlobalBalanceSchedule> List()
    {
        var outValue = new List<GlobalBalanceSchedule>();
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
public sealed class CachedGlobalBalanceScheduleStore(MemoryGlobalBalanceScheduleStore inner, GlobalBalanceScheduleCache cache, int hits, int misses) : IGlobalBalanceScheduleStore
{
    public MemoryGlobalBalanceScheduleStore Inner { get; set; } = inner;
    public GlobalBalanceScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalBalanceSchedule? Find(string code)
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

    public void Save(GlobalBalanceSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalBalanceSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global balance schedules. Empty fields match all.</summary>
public sealed record GlobalBalanceScheduleQuery(string Team, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalBalanceSchedule item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalBalanceSchedule> Apply(List<GlobalBalanceSchedule> items)
    {
        var matched = Collections.Filter(items, (GlobalBalanceSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global balance schedule store.</summary>
public static class GlobalBalanceScheduleStore
{
    public static IGlobalBalanceScheduleStore NewMemoryGlobalBalanceScheduleStore()
    {
        return new MemoryGlobalBalanceScheduleStore(
            new Dictionary<string, GlobalBalanceSchedule>(),
            new List<string>());
    }

    public static IGlobalBalanceScheduleStore NewCachedGlobalBalanceScheduleStore(MemoryGlobalBalanceScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalBalanceScheduleStore(inner, GlobalBalanceScheduleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<GlobalBalanceSchedule> FindGlobalBalanceSchedules(IGlobalBalanceScheduleStore store, GlobalBalanceScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
