namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form claims by code.</summary>
public interface ITaxFormClaimStore
{
    TaxFormClaim? Find(string code);
    void Save(TaxFormClaim item);
    bool Remove(string code);
    List<TaxFormClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormClaimStore(Dictionary<string, TaxFormClaim> items, List<string> order) : ITaxFormClaimStore
{
    public Dictionary<string, TaxFormClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormClaim item)
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

    public List<TaxFormClaim> List()
    {
        var outValue = new List<TaxFormClaim>();
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
public sealed class CachedTaxFormClaimStore(MemoryTaxFormClaimStore inner, TaxFormClaimCache cache, int hits, int misses) : ITaxFormClaimStore
{
    public MemoryTaxFormClaimStore Inner { get; set; } = inner;
    public TaxFormClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormClaim? Find(string code)
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

    public void Save(TaxFormClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form claims. Empty fields match all.</summary>
public sealed record TaxFormClaimQuery(string Country, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormClaim item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TaxFormClaim> Apply(List<TaxFormClaim> items)
    {
        var matched = Collections.Filter(items, (TaxFormClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form claim store.</summary>
public static class TaxFormClaimStore
{
    public static ITaxFormClaimStore NewMemoryTaxFormClaimStore()
    {
        return new MemoryTaxFormClaimStore(new Dictionary<string, TaxFormClaim>(), new List<string>());
    }

    public static ITaxFormClaimStore NewCachedTaxFormClaimStore(MemoryTaxFormClaimStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormClaimStore(inner, TaxFormClaimCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TaxFormClaim> FindTaxFormClaims(ITaxFormClaimStore store, TaxFormClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
