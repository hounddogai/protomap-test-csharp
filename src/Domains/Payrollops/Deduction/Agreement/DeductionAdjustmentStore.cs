namespace Acme.Workforce.Payrollops.Deduction.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction adjustments by code.</summary>
public interface IDeductionAdjustmentStore
{
    DeductionAdjustment? Find(string code);
    void Save(DeductionAdjustment item);
    bool Remove(string code);
    List<DeductionAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionAdjustmentStore(Dictionary<string, DeductionAdjustment> items, List<string> order) : IDeductionAdjustmentStore
{
    public Dictionary<string, DeductionAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionAdjustment item)
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

    public List<DeductionAdjustment> List()
    {
        var outValue = new List<DeductionAdjustment>();
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
public sealed class CachedDeductionAdjustmentStore(MemoryDeductionAdjustmentStore inner, DeductionAdjustmentCache cache, int hits, int misses) : IDeductionAdjustmentStore
{
    public MemoryDeductionAdjustmentStore Inner { get; set; } = inner;
    public DeductionAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionAdjustment? Find(string code)
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

    public void Save(DeductionAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction adjustments. Empty fields match all.</summary>
public sealed record DeductionAdjustmentQuery(string Source, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionAdjustment item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeductionAdjustment> Apply(List<DeductionAdjustment> items)
    {
        var matched = Collections.Filter(items, (DeductionAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction adjustment store.</summary>
public static class DeductionAdjustmentStore
{
    public static IDeductionAdjustmentStore NewMemoryDeductionAdjustmentStore()
    {
        return new MemoryDeductionAdjustmentStore(new Dictionary<string, DeductionAdjustment>(), new List<string>());
    }

    public static IDeductionAdjustmentStore NewCachedDeductionAdjustmentStore(MemoryDeductionAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionAdjustmentStore(inner, DeductionAdjustmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<DeductionAdjustment> FindDeductionAdjustments(IDeductionAdjustmentStore store, DeductionAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
