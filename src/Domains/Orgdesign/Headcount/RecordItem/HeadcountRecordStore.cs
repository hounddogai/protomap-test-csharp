namespace Acme.Workforce.Orgdesign.Headcount.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount records by code.</summary>
public interface IHeadcountRecordStore
{
    HeadcountRecord? Find(string code);
    void Save(HeadcountRecord item);
    bool Remove(string code);
    List<HeadcountRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountRecordStore(Dictionary<string, HeadcountRecord> items, List<string> order) : IHeadcountRecordStore
{
    public Dictionary<string, HeadcountRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountRecord item)
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

    public List<HeadcountRecord> List()
    {
        var outValue = new List<HeadcountRecord>();
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
public sealed class CachedHeadcountRecordStore(MemoryHeadcountRecordStore inner, HeadcountRecordCache cache, int hits, int misses) : IHeadcountRecordStore
{
    public MemoryHeadcountRecordStore Inner { get; set; } = inner;
    public HeadcountRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountRecord? Find(string code)
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

    public void Save(HeadcountRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount records. Empty fields match all.</summary>
public sealed record HeadcountRecordQuery(string Name, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountRecord item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountRecord> Apply(List<HeadcountRecord> items)
    {
        var matched = Collections.Filter(items, (HeadcountRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount record store.</summary>
public static class HeadcountRecordStore
{
    public static IHeadcountRecordStore NewMemoryHeadcountRecordStore()
    {
        return new MemoryHeadcountRecordStore(new Dictionary<string, HeadcountRecord>(), new List<string>());
    }

    public static IHeadcountRecordStore NewCachedHeadcountRecordStore(MemoryHeadcountRecordStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountRecordStore(inner, HeadcountRecordCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<HeadcountRecord> FindHeadcountRecords(IHeadcountRecordStore store, HeadcountRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
