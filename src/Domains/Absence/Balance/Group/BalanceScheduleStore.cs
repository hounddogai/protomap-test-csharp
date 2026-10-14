namespace Acme.Workforce.Absence.Balance.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance schedules by code.</summary>
public interface IBalanceScheduleStore
{
    BalanceSchedule? Find(string code);
    void Save(BalanceSchedule item);
    bool Remove(string code);
    List<BalanceSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalanceScheduleStore(Dictionary<string, BalanceSchedule> items, List<string> order) : IBalanceScheduleStore
{
    public Dictionary<string, BalanceSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalanceSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalanceSchedule item)
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

    public List<BalanceSchedule> List()
    {
        var outValue = new List<BalanceSchedule>();
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
public sealed class CachedBalanceScheduleStore(MemoryBalanceScheduleStore inner, BalanceScheduleCache cache, int hits, int misses) : IBalanceScheduleStore
{
    public MemoryBalanceScheduleStore Inner { get; set; } = inner;
    public BalanceScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalanceSchedule? Find(string code)
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

    public void Save(BalanceSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalanceSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance schedules. Empty fields match all.</summary>
public sealed record BalanceScheduleQuery(string Unit, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(BalanceSchedule item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BalanceSchedule> Apply(List<BalanceSchedule> items)
    {
        var matched = Collections.Filter(items, (BalanceSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance schedule store.</summary>
public static class BalanceScheduleStore
{
    public static IBalanceScheduleStore NewMemoryBalanceScheduleStore()
    {
        return new MemoryBalanceScheduleStore(new Dictionary<string, BalanceSchedule>(), new List<string>());
    }

    public static IBalanceScheduleStore NewCachedBalanceScheduleStore(MemoryBalanceScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedBalanceScheduleStore(inner, BalanceScheduleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BalanceSchedule> FindBalanceSchedules(IBalanceScheduleStore store, BalanceScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
