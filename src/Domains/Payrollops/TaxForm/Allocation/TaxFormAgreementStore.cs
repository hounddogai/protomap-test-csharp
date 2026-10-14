namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form agreements by code.</summary>
public interface ITaxFormAgreementStore
{
    TaxFormAgreement? Find(string code);
    void Save(TaxFormAgreement item);
    bool Remove(string code);
    List<TaxFormAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormAgreementStore(Dictionary<string, TaxFormAgreement> items, List<string> order) : ITaxFormAgreementStore
{
    public Dictionary<string, TaxFormAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormAgreement item)
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

    public List<TaxFormAgreement> List()
    {
        var outValue = new List<TaxFormAgreement>();
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
public sealed class CachedTaxFormAgreementStore(MemoryTaxFormAgreementStore inner, TaxFormAgreementCache cache, int hits, int misses) : ITaxFormAgreementStore
{
    public MemoryTaxFormAgreementStore Inner { get; set; } = inner;
    public TaxFormAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormAgreement? Find(string code)
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

    public void Save(TaxFormAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form agreements. Empty fields match all.</summary>
public sealed record TaxFormAgreementQuery(string Source, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormAgreement item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<TaxFormAgreement> Apply(List<TaxFormAgreement> items)
    {
        var matched = Collections.Filter(items, (TaxFormAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form agreement store.</summary>
public static class TaxFormAgreementStore
{
    public static ITaxFormAgreementStore NewMemoryTaxFormAgreementStore()
    {
        return new MemoryTaxFormAgreementStore(new Dictionary<string, TaxFormAgreement>(), new List<string>());
    }

    public static ITaxFormAgreementStore NewCachedTaxFormAgreementStore(MemoryTaxFormAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormAgreementStore(inner, TaxFormAgreementCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<TaxFormAgreement> FindTaxFormAgreements(ITaxFormAgreementStore store, TaxFormAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
