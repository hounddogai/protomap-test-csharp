namespace Acme.Workforce.Absence.LeaveRequest.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request records by code.</summary>
public interface ILeaveRequestRecordStore
{
    LeaveRequestRecord? Find(string code);
    void Save(LeaveRequestRecord item);
    bool Remove(string code);
    List<LeaveRequestRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestRecordStore(Dictionary<string, LeaveRequestRecord> items, List<string> order) : ILeaveRequestRecordStore
{
    public Dictionary<string, LeaveRequestRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestRecord item)
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

    public List<LeaveRequestRecord> List()
    {
        var outValue = new List<LeaveRequestRecord>();
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
public sealed class CachedLeaveRequestRecordStore(MemoryLeaveRequestRecordStore inner, LeaveRequestRecordCache cache, int hits, int misses) : ILeaveRequestRecordStore
{
    public MemoryLeaveRequestRecordStore Inner { get; set; } = inner;
    public LeaveRequestRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestRecord? Find(string code)
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

    public void Save(LeaveRequestRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request records. Empty fields match all.</summary>
public sealed record LeaveRequestRecordQuery(string Unit, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestRecord item)
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

    public List<LeaveRequestRecord> Apply(List<LeaveRequestRecord> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request record store.</summary>
public static class LeaveRequestRecordStore
{
    public static ILeaveRequestRecordStore NewMemoryLeaveRequestRecordStore()
    {
        return new MemoryLeaveRequestRecordStore(new Dictionary<string, LeaveRequestRecord>(), new List<string>());
    }

    public static ILeaveRequestRecordStore NewCachedLeaveRequestRecordStore(MemoryLeaveRequestRecordStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestRecordStore(inner, LeaveRequestRecordCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestRecord> FindLeaveRequestRecords(ILeaveRequestRecordStore store, LeaveRequestRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
