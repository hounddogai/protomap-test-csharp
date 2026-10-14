namespace Acme.Workforce.Payrollops.Garnishment.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped garnishment records by code.</summary>
public interface ICappedGarnishmentRecordStore
{
    CappedGarnishmentRecord? Find(string code);
    void Save(CappedGarnishmentRecord item);
    bool Remove(string code);
    List<CappedGarnishmentRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedGarnishmentRecordStore(Dictionary<string, CappedGarnishmentRecord> items, List<string> order) : ICappedGarnishmentRecordStore
{
    public Dictionary<string, CappedGarnishmentRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedGarnishmentRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedGarnishmentRecord item)
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

    public List<CappedGarnishmentRecord> List()
    {
        var outValue = new List<CappedGarnishmentRecord>();
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
public sealed class CachedCappedGarnishmentRecordStore(MemoryCappedGarnishmentRecordStore inner, CappedGarnishmentRecordCache cache, int hits, int misses) : ICappedGarnishmentRecordStore
{
    public MemoryCappedGarnishmentRecordStore Inner { get; set; } = inner;
    public CappedGarnishmentRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedGarnishmentRecord? Find(string code)
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

    public void Save(CappedGarnishmentRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedGarnishmentRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped garnishment records. Empty fields match all.</summary>
public sealed record CappedGarnishmentRecordQuery(string Name, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(CappedGarnishmentRecord item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedGarnishmentRecord> Apply(List<CappedGarnishmentRecord> items)
    {
        var matched = Collections.Filter(items, (CappedGarnishmentRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped garnishment record store.</summary>
public static class CappedGarnishmentRecordStore
{
    public static ICappedGarnishmentRecordStore NewMemoryCappedGarnishmentRecordStore()
    {
        return new MemoryCappedGarnishmentRecordStore(
            new Dictionary<string, CappedGarnishmentRecord>(),
            new List<string>());
    }

    public static ICappedGarnishmentRecordStore NewCachedCappedGarnishmentRecordStore(MemoryCappedGarnishmentRecordStore inner, int ttl, IClock clock)
    {
        return new CachedCappedGarnishmentRecordStore(
            inner,
            CappedGarnishmentRecordCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<CappedGarnishmentRecord> FindCappedGarnishmentRecords(ICappedGarnishmentRecordStore store, CappedGarnishmentRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
