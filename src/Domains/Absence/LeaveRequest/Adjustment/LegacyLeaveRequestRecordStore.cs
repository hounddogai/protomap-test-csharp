namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy leave request records by code.</summary>
public interface ILegacyLeaveRequestRecordStore
{
    LegacyLeaveRequestRecord? Find(string code);
    void Save(LegacyLeaveRequestRecord item);
    bool Remove(string code);
    List<LegacyLeaveRequestRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyLeaveRequestRecordStore(Dictionary<string, LegacyLeaveRequestRecord> items, List<string> order) : ILegacyLeaveRequestRecordStore
{
    public Dictionary<string, LegacyLeaveRequestRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyLeaveRequestRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyLeaveRequestRecord item)
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

    public List<LegacyLeaveRequestRecord> List()
    {
        var outValue = new List<LegacyLeaveRequestRecord>();
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
public sealed class CachedLegacyLeaveRequestRecordStore(MemoryLegacyLeaveRequestRecordStore inner, LegacyLeaveRequestRecordCache cache, int hits, int misses) : ILegacyLeaveRequestRecordStore
{
    public MemoryLegacyLeaveRequestRecordStore Inner { get; set; } = inner;
    public LegacyLeaveRequestRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyLeaveRequestRecord? Find(string code)
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

    public void Save(LegacyLeaveRequestRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyLeaveRequestRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy leave request records. Empty fields match all.</summary>
public sealed record LegacyLeaveRequestRecordQuery(string Segment, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyLeaveRequestRecord item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyLeaveRequestRecord> Apply(List<LegacyLeaveRequestRecord> items)
    {
        var matched = Collections.Filter(items, (LegacyLeaveRequestRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy leave request record store.</summary>
public static class LegacyLeaveRequestRecordStore
{
    public static ILegacyLeaveRequestRecordStore NewMemoryLegacyLeaveRequestRecordStore()
    {
        return new MemoryLegacyLeaveRequestRecordStore(
            new Dictionary<string, LegacyLeaveRequestRecord>(),
            new List<string>());
    }

    public static ILegacyLeaveRequestRecordStore NewCachedLegacyLeaveRequestRecordStore(MemoryLegacyLeaveRequestRecordStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyLeaveRequestRecordStore(
            inner,
            LegacyLeaveRequestRecordCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LegacyLeaveRequestRecord> FindLegacyLeaveRequestRecords(ILegacyLeaveRequestRecordStore store, LegacyLeaveRequestRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
