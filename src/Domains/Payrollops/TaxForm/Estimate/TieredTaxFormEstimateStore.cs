namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered tax form estimates by code.</summary>
public interface ITieredTaxFormEstimateStore
{
    TieredTaxFormEstimate? Find(string code);
    void Save(TieredTaxFormEstimate item);
    bool Remove(string code);
    List<TieredTaxFormEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredTaxFormEstimateStore(Dictionary<string, TieredTaxFormEstimate> items, List<string> order) : ITieredTaxFormEstimateStore
{
    public Dictionary<string, TieredTaxFormEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredTaxFormEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredTaxFormEstimate item)
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

    public List<TieredTaxFormEstimate> List()
    {
        var outValue = new List<TieredTaxFormEstimate>();
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
public sealed class CachedTieredTaxFormEstimateStore(MemoryTieredTaxFormEstimateStore inner, TieredTaxFormEstimateCache cache, int hits, int misses) : ITieredTaxFormEstimateStore
{
    public MemoryTieredTaxFormEstimateStore Inner { get; set; } = inner;
    public TieredTaxFormEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredTaxFormEstimate? Find(string code)
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

    public void Save(TieredTaxFormEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredTaxFormEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered tax form estimates. Empty fields match all.</summary>
public sealed record TieredTaxFormEstimateQuery(string Name, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(TieredTaxFormEstimate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<TieredTaxFormEstimate> Apply(List<TieredTaxFormEstimate> items)
    {
        var matched = Collections.Filter(items, (TieredTaxFormEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered tax form estimate store.</summary>
public static class TieredTaxFormEstimateStore
{
    public static ITieredTaxFormEstimateStore NewMemoryTieredTaxFormEstimateStore()
    {
        return new MemoryTieredTaxFormEstimateStore(
            new Dictionary<string, TieredTaxFormEstimate>(),
            new List<string>());
    }

    public static ITieredTaxFormEstimateStore NewCachedTieredTaxFormEstimateStore(MemoryTieredTaxFormEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedTieredTaxFormEstimateStore(inner, TieredTaxFormEstimateCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<TieredTaxFormEstimate> FindTieredTaxFormEstimates(ITieredTaxFormEstimateStore store, TieredTaxFormEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
