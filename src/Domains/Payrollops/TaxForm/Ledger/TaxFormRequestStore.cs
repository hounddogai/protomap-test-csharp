namespace Acme.Workforce.Payrollops.TaxForm.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form requests by code.</summary>
public interface ITaxFormRequestStore
{
    TaxFormRequest? Find(string code);
    void Save(TaxFormRequest item);
    bool Remove(string code);
    List<TaxFormRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormRequestStore(Dictionary<string, TaxFormRequest> items, List<string> order) : ITaxFormRequestStore
{
    public Dictionary<string, TaxFormRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormRequest item)
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

    public List<TaxFormRequest> List()
    {
        var outValue = new List<TaxFormRequest>();
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
public sealed class CachedTaxFormRequestStore(MemoryTaxFormRequestStore inner, TaxFormRequestCache cache, int hits, int misses) : ITaxFormRequestStore
{
    public MemoryTaxFormRequestStore Inner { get; set; } = inner;
    public TaxFormRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormRequest? Find(string code)
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

    public void Save(TaxFormRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form requests. Empty fields match all.</summary>
public sealed record TaxFormRequestQuery(string Region, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormRequest item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<TaxFormRequest> Apply(List<TaxFormRequest> items)
    {
        var matched = Collections.Filter(items, (TaxFormRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form request store.</summary>
public static class TaxFormRequestStore
{
    public static ITaxFormRequestStore NewMemoryTaxFormRequestStore()
    {
        return new MemoryTaxFormRequestStore(new Dictionary<string, TaxFormRequest>(), new List<string>());
    }

    public static ITaxFormRequestStore NewCachedTaxFormRequestStore(MemoryTaxFormRequestStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormRequestStore(inner, TaxFormRequestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<TaxFormRequest> FindTaxFormRequests(ITaxFormRequestStore store, TaxFormRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
