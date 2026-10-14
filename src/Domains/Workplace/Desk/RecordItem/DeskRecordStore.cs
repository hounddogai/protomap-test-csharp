namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk records by code.</summary>
public interface IDeskRecordStore
{
    DeskRecord? Find(string code);
    void Save(DeskRecord item);
    bool Remove(string code);
    List<DeskRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskRecordStore(Dictionary<string, DeskRecord> items, List<string> order) : IDeskRecordStore
{
    public Dictionary<string, DeskRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskRecord item)
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

    public List<DeskRecord> List()
    {
        var outValue = new List<DeskRecord>();
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
public sealed class CachedDeskRecordStore(MemoryDeskRecordStore inner, DeskRecordCache cache, int hits, int misses) : IDeskRecordStore
{
    public MemoryDeskRecordStore Inner { get; set; } = inner;
    public DeskRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskRecord? Find(string code)
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

    public void Save(DeskRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk records. Empty fields match all.</summary>
public sealed record DeskRecordQuery(string Source, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(DeskRecord item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskRecord> Apply(List<DeskRecord> items)
    {
        var matched = Collections.Filter(items, (DeskRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk record store.</summary>
public static class DeskRecordStore
{
    public static IDeskRecordStore NewMemoryDeskRecordStore()
    {
        return new MemoryDeskRecordStore(new Dictionary<string, DeskRecord>(), new List<string>());
    }

    public static IDeskRecordStore NewCachedDeskRecordStore(MemoryDeskRecordStore inner, int ttl, IClock clock)
    {
        return new CachedDeskRecordStore(inner, DeskRecordCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<DeskRecord> FindDeskRecords(IDeskRecordStore store, DeskRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
