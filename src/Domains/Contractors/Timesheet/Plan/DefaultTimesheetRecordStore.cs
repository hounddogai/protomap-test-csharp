namespace Acme.Workforce.Contractors.Timesheet.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default timesheet records by code.</summary>
public interface IDefaultTimesheetRecordStore
{
    DefaultTimesheetRecord? Find(string code);
    void Save(DefaultTimesheetRecord item);
    bool Remove(string code);
    List<DefaultTimesheetRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultTimesheetRecordStore(Dictionary<string, DefaultTimesheetRecord> items, List<string> order) : IDefaultTimesheetRecordStore
{
    public Dictionary<string, DefaultTimesheetRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultTimesheetRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultTimesheetRecord item)
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

    public List<DefaultTimesheetRecord> List()
    {
        var outValue = new List<DefaultTimesheetRecord>();
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
public sealed class CachedDefaultTimesheetRecordStore(MemoryDefaultTimesheetRecordStore inner, DefaultTimesheetRecordCache cache, int hits, int misses) : IDefaultTimesheetRecordStore
{
    public MemoryDefaultTimesheetRecordStore Inner { get; set; } = inner;
    public DefaultTimesheetRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultTimesheetRecord? Find(string code)
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

    public void Save(DefaultTimesheetRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultTimesheetRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default timesheet records. Empty fields match all.</summary>
public sealed record DefaultTimesheetRecordQuery(string Category, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultTimesheetRecord item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultTimesheetRecord> Apply(List<DefaultTimesheetRecord> items)
    {
        var matched = Collections.Filter(items, (DefaultTimesheetRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default timesheet record store.</summary>
public static class DefaultTimesheetRecordStore
{
    public static IDefaultTimesheetRecordStore NewMemoryDefaultTimesheetRecordStore()
    {
        return new MemoryDefaultTimesheetRecordStore(
            new Dictionary<string, DefaultTimesheetRecord>(),
            new List<string>());
    }

    public static IDefaultTimesheetRecordStore NewCachedDefaultTimesheetRecordStore(MemoryDefaultTimesheetRecordStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultTimesheetRecordStore(
            inner,
            DefaultTimesheetRecordCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<DefaultTimesheetRecord> FindDefaultTimesheetRecords(IDefaultTimesheetRecordStore store, DefaultTimesheetRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
