namespace Acme.Workforce.Payrollops.TaxForm.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form mappings by code.</summary>
public interface ITaxFormMappingStore
{
    TaxFormMapping? Find(string code);
    void Save(TaxFormMapping item);
    bool Remove(string code);
    List<TaxFormMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormMappingStore(Dictionary<string, TaxFormMapping> items, List<string> order) : ITaxFormMappingStore
{
    public Dictionary<string, TaxFormMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormMapping item)
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

    public List<TaxFormMapping> List()
    {
        var outValue = new List<TaxFormMapping>();
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
public sealed class CachedTaxFormMappingStore(MemoryTaxFormMappingStore inner, TaxFormMappingCache cache, int hits, int misses) : ITaxFormMappingStore
{
    public MemoryTaxFormMappingStore Inner { get; set; } = inner;
    public TaxFormMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormMapping? Find(string code)
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

    public void Save(TaxFormMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form mappings. Empty fields match all.</summary>
public sealed record TaxFormMappingQuery(string Region, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormMapping item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TaxFormMapping> Apply(List<TaxFormMapping> items)
    {
        var matched = Collections.Filter(items, (TaxFormMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form mapping store.</summary>
public static class TaxFormMappingStore
{
    public static ITaxFormMappingStore NewMemoryTaxFormMappingStore()
    {
        return new MemoryTaxFormMappingStore(new Dictionary<string, TaxFormMapping>(), new List<string>());
    }

    public static ITaxFormMappingStore NewCachedTaxFormMappingStore(MemoryTaxFormMappingStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormMappingStore(inner, TaxFormMappingCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<TaxFormMapping> FindTaxFormMappings(ITaxFormMappingStore store, TaxFormMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
