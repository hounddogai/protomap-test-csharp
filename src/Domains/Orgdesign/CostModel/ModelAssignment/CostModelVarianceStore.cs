namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps cost model variances by code.</summary>
public interface ICostModelVarianceStore
{
    CostModelVariance? Find(string code);
    void Save(CostModelVariance item);
    bool Remove(string code);
    List<CostModelVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCostModelVarianceStore(Dictionary<string, CostModelVariance> items, List<string> order) : ICostModelVarianceStore
{
    public Dictionary<string, CostModelVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CostModelVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CostModelVariance item)
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

    public List<CostModelVariance> List()
    {
        var outValue = new List<CostModelVariance>();
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
public sealed class CachedCostModelVarianceStore(MemoryCostModelVarianceStore inner, CostModelVarianceCache cache, int hits, int misses) : ICostModelVarianceStore
{
    public MemoryCostModelVarianceStore Inner { get; set; } = inner;
    public CostModelVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CostModelVariance? Find(string code)
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

    public void Save(CostModelVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CostModelVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects cost model variances. Empty fields match all.</summary>
public sealed record CostModelVarianceQuery(string Unit, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(CostModelVariance item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CostModelVariance> Apply(List<CostModelVariance> items)
    {
        var matched = Collections.Filter(items, (CostModelVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of cost model variance store.</summary>
public static class CostModelVarianceStore
{
    public static ICostModelVarianceStore NewMemoryCostModelVarianceStore()
    {
        return new MemoryCostModelVarianceStore(new Dictionary<string, CostModelVariance>(), new List<string>());
    }

    public static ICostModelVarianceStore NewCachedCostModelVarianceStore(MemoryCostModelVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedCostModelVarianceStore(inner, CostModelVarianceCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CostModelVariance> FindCostModelVariances(ICostModelVarianceStore store, CostModelVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
