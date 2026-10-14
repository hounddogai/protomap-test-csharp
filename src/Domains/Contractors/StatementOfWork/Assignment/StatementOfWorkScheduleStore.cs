namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work schedules by code.</summary>
public interface IStatementOfWorkScheduleStore
{
    StatementOfWorkSchedule? Find(string code);
    void Save(StatementOfWorkSchedule item);
    bool Remove(string code);
    List<StatementOfWorkSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkScheduleStore(Dictionary<string, StatementOfWorkSchedule> items, List<string> order) : IStatementOfWorkScheduleStore
{
    public Dictionary<string, StatementOfWorkSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkSchedule item)
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

    public List<StatementOfWorkSchedule> List()
    {
        var outValue = new List<StatementOfWorkSchedule>();
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
public sealed class CachedStatementOfWorkScheduleStore(MemoryStatementOfWorkScheduleStore inner, StatementOfWorkScheduleCache cache, int hits, int misses) : IStatementOfWorkScheduleStore
{
    public MemoryStatementOfWorkScheduleStore Inner { get; set; } = inner;
    public StatementOfWorkScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkSchedule? Find(string code)
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

    public void Save(StatementOfWorkSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work schedules. Empty fields match all.</summary>
public sealed record StatementOfWorkScheduleQuery(string Name, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkSchedule item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkSchedule> Apply(List<StatementOfWorkSchedule> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work schedule store.</summary>
public static class StatementOfWorkScheduleStore
{
    public static IStatementOfWorkScheduleStore NewMemoryStatementOfWorkScheduleStore()
    {
        return new MemoryStatementOfWorkScheduleStore(
            new Dictionary<string, StatementOfWorkSchedule>(),
            new List<string>());
    }

    public static IStatementOfWorkScheduleStore NewCachedStatementOfWorkScheduleStore(MemoryStatementOfWorkScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkScheduleStore(
            inner,
            StatementOfWorkScheduleCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StatementOfWorkSchedule> FindStatementOfWorkSchedules(IStatementOfWorkScheduleStore store, StatementOfWorkScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
