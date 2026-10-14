namespace Acme.Workforce.Payrollops.TaxForm.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form baselines by code.</summary>
public interface ITaxFormBaselineStore
{
    TaxFormBaseline? Find(string code);
    void Save(TaxFormBaseline item);
    bool Remove(string code);
    List<TaxFormBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormBaselineStore(Dictionary<string, TaxFormBaseline> items, List<string> order) : ITaxFormBaselineStore
{
    public Dictionary<string, TaxFormBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormBaseline item)
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

    public List<TaxFormBaseline> List()
    {
        var outValue = new List<TaxFormBaseline>();
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
public sealed class CachedTaxFormBaselineStore(MemoryTaxFormBaselineStore inner, TaxFormBaselineCache cache, int hits, int misses) : ITaxFormBaselineStore
{
    public MemoryTaxFormBaselineStore Inner { get; set; } = inner;
    public TaxFormBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormBaseline? Find(string code)
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

    public void Save(TaxFormBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form baselines. Empty fields match all.</summary>
public sealed record TaxFormBaselineQuery(string Country, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormBaseline item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<TaxFormBaseline> Apply(List<TaxFormBaseline> items)
    {
        var matched = Collections.Filter(items, (TaxFormBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form baseline store.</summary>
public static class TaxFormBaselineStore
{
    public static ITaxFormBaselineStore NewMemoryTaxFormBaselineStore()
    {
        return new MemoryTaxFormBaselineStore(new Dictionary<string, TaxFormBaseline>(), new List<string>());
    }

    public static ITaxFormBaselineStore NewCachedTaxFormBaselineStore(MemoryTaxFormBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormBaselineStore(inner, TaxFormBaselineCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TaxFormBaseline> FindTaxFormBaselines(ITaxFormBaselineStore store, TaxFormBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
