namespace Acme.Workforce.Payrollops.Deduction.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction estimates by code.</summary>
public interface IDeductionEstimateStore
{
    DeductionEstimate? Find(string code);
    void Save(DeductionEstimate item);
    bool Remove(string code);
    List<DeductionEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionEstimateStore(Dictionary<string, DeductionEstimate> items, List<string> order) : IDeductionEstimateStore
{
    public Dictionary<string, DeductionEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionEstimate item)
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

    public List<DeductionEstimate> List()
    {
        var outValue = new List<DeductionEstimate>();
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
public sealed class CachedDeductionEstimateStore(MemoryDeductionEstimateStore inner, DeductionEstimateCache cache, int hits, int misses) : IDeductionEstimateStore
{
    public MemoryDeductionEstimateStore Inner { get; set; } = inner;
    public DeductionEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionEstimate? Find(string code)
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

    public void Save(DeductionEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction estimates. Empty fields match all.</summary>
public sealed record DeductionEstimateQuery(string Locale, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionEstimate item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeductionEstimate> Apply(List<DeductionEstimate> items)
    {
        var matched = Collections.Filter(items, (DeductionEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction estimate store.</summary>
public static class DeductionEstimateStore
{
    public static IDeductionEstimateStore NewMemoryDeductionEstimateStore()
    {
        return new MemoryDeductionEstimateStore(new Dictionary<string, DeductionEstimate>(), new List<string>());
    }

    public static IDeductionEstimateStore NewCachedDeductionEstimateStore(MemoryDeductionEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionEstimateStore(inner, DeductionEstimateCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DeductionEstimate> FindDeductionEstimates(IDeductionEstimateStore store, DeductionEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
