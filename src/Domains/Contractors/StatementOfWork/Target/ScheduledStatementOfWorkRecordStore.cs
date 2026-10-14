namespace Acme.Workforce.Contractors.StatementOfWork.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled statement of work records by code.</summary>
public interface IScheduledStatementOfWorkRecordStore
{
    ScheduledStatementOfWorkRecord? Find(string code);
    void Save(ScheduledStatementOfWorkRecord item);
    bool Remove(string code);
    List<ScheduledStatementOfWorkRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledStatementOfWorkRecordStore(Dictionary<string, ScheduledStatementOfWorkRecord> items, List<string> order) : IScheduledStatementOfWorkRecordStore
{
    public Dictionary<string, ScheduledStatementOfWorkRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledStatementOfWorkRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledStatementOfWorkRecord item)
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

    public List<ScheduledStatementOfWorkRecord> List()
    {
        var outValue = new List<ScheduledStatementOfWorkRecord>();
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
public sealed class CachedScheduledStatementOfWorkRecordStore(MemoryScheduledStatementOfWorkRecordStore inner, ScheduledStatementOfWorkRecordCache cache, int hits, int misses) : IScheduledStatementOfWorkRecordStore
{
    public MemoryScheduledStatementOfWorkRecordStore Inner { get; set; } = inner;
    public ScheduledStatementOfWorkRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledStatementOfWorkRecord? Find(string code)
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

    public void Save(ScheduledStatementOfWorkRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledStatementOfWorkRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled statement of work records. Empty fields match all.</summary>
public sealed record ScheduledStatementOfWorkRecordQuery(string Name, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledStatementOfWorkRecord item)
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

    public List<ScheduledStatementOfWorkRecord> Apply(List<ScheduledStatementOfWorkRecord> items)
    {
        var matched = Collections.Filter(items, (ScheduledStatementOfWorkRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled statement of work record store.</summary>
public static class ScheduledStatementOfWorkRecordStore
{
    public static IScheduledStatementOfWorkRecordStore NewMemoryScheduledStatementOfWorkRecordStore()
    {
        return new MemoryScheduledStatementOfWorkRecordStore(
            new Dictionary<string, ScheduledStatementOfWorkRecord>(),
            new List<string>());
    }

    public static IScheduledStatementOfWorkRecordStore NewCachedScheduledStatementOfWorkRecordStore(MemoryScheduledStatementOfWorkRecordStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledStatementOfWorkRecordStore(
            inner,
            ScheduledStatementOfWorkRecordCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledStatementOfWorkRecord> FindScheduledStatementOfWorkRecords(IScheduledStatementOfWorkRecordStore store, ScheduledStatementOfWorkRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
