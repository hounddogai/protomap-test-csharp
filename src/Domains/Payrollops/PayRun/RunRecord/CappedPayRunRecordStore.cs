namespace Acme.Workforce.Payrollops.PayRun.RunRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped pay run records by code.</summary>
public interface ICappedPayRunRecordStore
{
    CappedPayRunRecord? Find(string code);
    void Save(CappedPayRunRecord item);
    bool Remove(string code);
    List<CappedPayRunRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedPayRunRecordStore(Dictionary<string, CappedPayRunRecord> items, List<string> order) : ICappedPayRunRecordStore
{
    public Dictionary<string, CappedPayRunRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedPayRunRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedPayRunRecord item)
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

    public List<CappedPayRunRecord> List()
    {
        var outValue = new List<CappedPayRunRecord>();
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
public sealed class CachedCappedPayRunRecordStore(MemoryCappedPayRunRecordStore inner, CappedPayRunRecordCache cache, int hits, int misses) : ICappedPayRunRecordStore
{
    public MemoryCappedPayRunRecordStore Inner { get; set; } = inner;
    public CappedPayRunRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedPayRunRecord? Find(string code)
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

    public void Save(CappedPayRunRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedPayRunRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped pay run records. Empty fields match all.</summary>
public sealed record CappedPayRunRecordQuery(string Name, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(CappedPayRunRecord item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedPayRunRecord> Apply(List<CappedPayRunRecord> items)
    {
        var matched = Collections.Filter(items, (CappedPayRunRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped pay run record store.</summary>
public static class CappedPayRunRecordStore
{
    public static ICappedPayRunRecordStore NewMemoryCappedPayRunRecordStore()
    {
        return new MemoryCappedPayRunRecordStore(new Dictionary<string, CappedPayRunRecord>(), new List<string>());
    }

    public static ICappedPayRunRecordStore NewCachedCappedPayRunRecordStore(MemoryCappedPayRunRecordStore inner, int ttl, IClock clock)
    {
        return new CachedCappedPayRunRecordStore(inner, CappedPayRunRecordCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<CappedPayRunRecord> FindCappedPayRunRecords(ICappedPayRunRecordStore store, CappedPayRunRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
