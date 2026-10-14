namespace Acme.Workforce.Payrollops.Deduction.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction variances by code.</summary>
public interface IDeductionVarianceStore
{
    DeductionVariance? Find(string code);
    void Save(DeductionVariance item);
    bool Remove(string code);
    List<DeductionVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionVarianceStore(Dictionary<string, DeductionVariance> items, List<string> order) : IDeductionVarianceStore
{
    public Dictionary<string, DeductionVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionVariance item)
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

    public List<DeductionVariance> List()
    {
        var outValue = new List<DeductionVariance>();
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
public sealed class CachedDeductionVarianceStore(MemoryDeductionVarianceStore inner, DeductionVarianceCache cache, int hits, int misses) : IDeductionVarianceStore
{
    public MemoryDeductionVarianceStore Inner { get; set; } = inner;
    public DeductionVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionVariance? Find(string code)
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

    public void Save(DeductionVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction variances. Empty fields match all.</summary>
public sealed record DeductionVarianceQuery(string Title, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionVariance item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<DeductionVariance> Apply(List<DeductionVariance> items)
    {
        var matched = Collections.Filter(items, (DeductionVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction variance store.</summary>
public static class DeductionVarianceStore
{
    public static IDeductionVarianceStore NewMemoryDeductionVarianceStore()
    {
        return new MemoryDeductionVarianceStore(new Dictionary<string, DeductionVariance>(), new List<string>());
    }

    public static IDeductionVarianceStore NewCachedDeductionVarianceStore(MemoryDeductionVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionVarianceStore(inner, DeductionVarianceCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DeductionVariance> FindDeductionVariances(IDeductionVarianceStore store, DeductionVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
