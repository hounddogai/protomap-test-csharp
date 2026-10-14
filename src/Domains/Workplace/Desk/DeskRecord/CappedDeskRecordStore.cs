namespace Acme.Workforce.Workplace.Desk.DeskRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped desk records by code.</summary>
public interface ICappedDeskRecordStore
{
    CappedDeskRecord? Find(string code);
    void Save(CappedDeskRecord item);
    bool Remove(string code);
    List<CappedDeskRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedDeskRecordStore(Dictionary<string, CappedDeskRecord> items, List<string> order) : ICappedDeskRecordStore
{
    public Dictionary<string, CappedDeskRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedDeskRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedDeskRecord item)
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

    public List<CappedDeskRecord> List()
    {
        var outValue = new List<CappedDeskRecord>();
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
public sealed class CachedCappedDeskRecordStore(MemoryCappedDeskRecordStore inner, CappedDeskRecordCache cache, int hits, int misses) : ICappedDeskRecordStore
{
    public MemoryCappedDeskRecordStore Inner { get; set; } = inner;
    public CappedDeskRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedDeskRecord? Find(string code)
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

    public void Save(CappedDeskRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedDeskRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped desk records. Empty fields match all.</summary>
public sealed record CappedDeskRecordQuery(string Segment, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(CappedDeskRecord item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedDeskRecord> Apply(List<CappedDeskRecord> items)
    {
        var matched = Collections.Filter(items, (CappedDeskRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped desk record store.</summary>
public static class CappedDeskRecordStore
{
    public static ICappedDeskRecordStore NewMemoryCappedDeskRecordStore()
    {
        return new MemoryCappedDeskRecordStore(new Dictionary<string, CappedDeskRecord>(), new List<string>());
    }

    public static ICappedDeskRecordStore NewCachedCappedDeskRecordStore(MemoryCappedDeskRecordStore inner, int ttl, IClock clock)
    {
        return new CachedCappedDeskRecordStore(inner, CappedDeskRecordCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<CappedDeskRecord> FindCappedDeskRecords(ICappedDeskRecordStore store, CappedDeskRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
