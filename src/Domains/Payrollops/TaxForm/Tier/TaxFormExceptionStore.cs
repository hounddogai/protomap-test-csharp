namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form exceptions by code.</summary>
public interface ITaxFormExceptionStore
{
    TaxFormException? Find(string code);
    void Save(TaxFormException item);
    bool Remove(string code);
    List<TaxFormException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormExceptionStore(Dictionary<string, TaxFormException> items, List<string> order) : ITaxFormExceptionStore
{
    public Dictionary<string, TaxFormException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormException item)
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

    public List<TaxFormException> List()
    {
        var outValue = new List<TaxFormException>();
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
public sealed class CachedTaxFormExceptionStore(MemoryTaxFormExceptionStore inner, TaxFormExceptionCache cache, int hits, int misses) : ITaxFormExceptionStore
{
    public MemoryTaxFormExceptionStore Inner { get; set; } = inner;
    public TaxFormExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormException? Find(string code)
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

    public void Save(TaxFormException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form exceptions. Empty fields match all.</summary>
public sealed record TaxFormExceptionQuery(string Category, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormException item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<TaxFormException> Apply(List<TaxFormException> items)
    {
        var matched = Collections.Filter(items, (TaxFormException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form exception store.</summary>
public static class TaxFormExceptionStore
{
    public static ITaxFormExceptionStore NewMemoryTaxFormExceptionStore()
    {
        return new MemoryTaxFormExceptionStore(new Dictionary<string, TaxFormException>(), new List<string>());
    }

    public static ITaxFormExceptionStore NewCachedTaxFormExceptionStore(MemoryTaxFormExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormExceptionStore(inner, TaxFormExceptionCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TaxFormException> FindTaxFormExceptions(ITaxFormExceptionStore store, TaxFormExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
