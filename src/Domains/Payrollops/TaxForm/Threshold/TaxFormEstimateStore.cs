namespace Acme.Workforce.Payrollops.TaxForm.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form estimates by code.</summary>
public interface ITaxFormEstimateStore
{
    TaxFormEstimate? Find(string code);
    void Save(TaxFormEstimate item);
    bool Remove(string code);
    List<TaxFormEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormEstimateStore(Dictionary<string, TaxFormEstimate> items, List<string> order) : ITaxFormEstimateStore
{
    public Dictionary<string, TaxFormEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormEstimate item)
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

    public List<TaxFormEstimate> List()
    {
        var outValue = new List<TaxFormEstimate>();
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
public sealed class CachedTaxFormEstimateStore(MemoryTaxFormEstimateStore inner, TaxFormEstimateCache cache, int hits, int misses) : ITaxFormEstimateStore
{
    public MemoryTaxFormEstimateStore Inner { get; set; } = inner;
    public TaxFormEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormEstimate? Find(string code)
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

    public void Save(TaxFormEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form estimates. Empty fields match all.</summary>
public sealed record TaxFormEstimateQuery(string Title, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormEstimate item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TaxFormEstimate> Apply(List<TaxFormEstimate> items)
    {
        var matched = Collections.Filter(items, (TaxFormEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form estimate store.</summary>
public static class TaxFormEstimateStore
{
    public static ITaxFormEstimateStore NewMemoryTaxFormEstimateStore()
    {
        return new MemoryTaxFormEstimateStore(new Dictionary<string, TaxFormEstimate>(), new List<string>());
    }

    public static ITaxFormEstimateStore NewCachedTaxFormEstimateStore(MemoryTaxFormEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormEstimateStore(inner, TaxFormEstimateCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TaxFormEstimate> FindTaxFormEstimates(ITaxFormEstimateStore store, TaxFormEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
