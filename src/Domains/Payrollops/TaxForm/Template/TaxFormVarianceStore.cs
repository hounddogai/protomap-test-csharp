namespace Acme.Workforce.Payrollops.TaxForm.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form variances by code.</summary>
public interface ITaxFormVarianceStore
{
    TaxFormVariance? Find(string code);
    void Save(TaxFormVariance item);
    bool Remove(string code);
    List<TaxFormVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormVarianceStore(Dictionary<string, TaxFormVariance> items, List<string> order) : ITaxFormVarianceStore
{
    public Dictionary<string, TaxFormVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormVariance item)
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

    public List<TaxFormVariance> List()
    {
        var outValue = new List<TaxFormVariance>();
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
public sealed class CachedTaxFormVarianceStore(MemoryTaxFormVarianceStore inner, TaxFormVarianceCache cache, int hits, int misses) : ITaxFormVarianceStore
{
    public MemoryTaxFormVarianceStore Inner { get; set; } = inner;
    public TaxFormVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormVariance? Find(string code)
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

    public void Save(TaxFormVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form variances. Empty fields match all.</summary>
public sealed record TaxFormVarianceQuery(string Locale, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormVariance item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TaxFormVariance> Apply(List<TaxFormVariance> items)
    {
        var matched = Collections.Filter(items, (TaxFormVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form variance store.</summary>
public static class TaxFormVarianceStore
{
    public static ITaxFormVarianceStore NewMemoryTaxFormVarianceStore()
    {
        return new MemoryTaxFormVarianceStore(new Dictionary<string, TaxFormVariance>(), new List<string>());
    }

    public static ITaxFormVarianceStore NewCachedTaxFormVarianceStore(MemoryTaxFormVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormVarianceStore(inner, TaxFormVarianceCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TaxFormVariance> FindTaxFormVariances(ITaxFormVarianceStore store, TaxFormVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
