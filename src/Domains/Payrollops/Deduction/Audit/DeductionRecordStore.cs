namespace Acme.Workforce.Payrollops.Deduction.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction records by code.</summary>
public interface IDeductionRecordStore
{
    DeductionRecord? Find(string code);
    void Save(DeductionRecord item);
    bool Remove(string code);
    List<DeductionRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionRecordStore(Dictionary<string, DeductionRecord> items, List<string> order) : IDeductionRecordStore
{
    public Dictionary<string, DeductionRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionRecord item)
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

    public List<DeductionRecord> List()
    {
        var outValue = new List<DeductionRecord>();
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
public sealed class CachedDeductionRecordStore(MemoryDeductionRecordStore inner, DeductionRecordCache cache, int hits, int misses) : IDeductionRecordStore
{
    public MemoryDeductionRecordStore Inner { get; set; } = inner;
    public DeductionRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionRecord? Find(string code)
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

    public void Save(DeductionRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction records. Empty fields match all.</summary>
public sealed record DeductionRecordQuery(string Segment, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionRecord item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeductionRecord> Apply(List<DeductionRecord> items)
    {
        var matched = Collections.Filter(items, (DeductionRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction record store.</summary>
public static class DeductionRecordStore
{
    public static IDeductionRecordStore NewMemoryDeductionRecordStore()
    {
        return new MemoryDeductionRecordStore(new Dictionary<string, DeductionRecord>(), new List<string>());
    }

    public static IDeductionRecordStore NewCachedDeductionRecordStore(MemoryDeductionRecordStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionRecordStore(inner, DeductionRecordCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<DeductionRecord> FindDeductionRecords(IDeductionRecordStore store, DeductionRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
