namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form records by code.</summary>
public interface ITaxFormRecordStore
{
    TaxFormRecord? Find(string code);
    void Save(TaxFormRecord item);
    bool Remove(string code);
    List<TaxFormRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormRecordStore(Dictionary<string, TaxFormRecord> items, List<string> order) : ITaxFormRecordStore
{
    public Dictionary<string, TaxFormRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormRecord item)
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

    public List<TaxFormRecord> List()
    {
        var outValue = new List<TaxFormRecord>();
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
public sealed class CachedTaxFormRecordStore(MemoryTaxFormRecordStore inner, TaxFormRecordCache cache, int hits, int misses) : ITaxFormRecordStore
{
    public MemoryTaxFormRecordStore Inner { get; set; } = inner;
    public TaxFormRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormRecord? Find(string code)
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

    public void Save(TaxFormRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form records. Empty fields match all.</summary>
public sealed record TaxFormRecordQuery(string Label, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormRecord item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<TaxFormRecord> Apply(List<TaxFormRecord> items)
    {
        var matched = Collections.Filter(items, (TaxFormRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form record store.</summary>
public static class TaxFormRecordStore
{
    public static ITaxFormRecordStore NewMemoryTaxFormRecordStore()
    {
        return new MemoryTaxFormRecordStore(new Dictionary<string, TaxFormRecord>(), new List<string>());
    }

    public static ITaxFormRecordStore NewCachedTaxFormRecordStore(MemoryTaxFormRecordStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormRecordStore(inner, TaxFormRecordCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TaxFormRecord> FindTaxFormRecords(ITaxFormRecordStore store, TaxFormRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
