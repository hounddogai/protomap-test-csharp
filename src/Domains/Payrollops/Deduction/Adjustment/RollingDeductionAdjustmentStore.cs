namespace Acme.Workforce.Payrollops.Deduction.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling deduction adjustments by code.</summary>
public interface IRollingDeductionAdjustmentStore
{
    RollingDeductionAdjustment? Find(string code);
    void Save(RollingDeductionAdjustment item);
    bool Remove(string code);
    List<RollingDeductionAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingDeductionAdjustmentStore(Dictionary<string, RollingDeductionAdjustment> items, List<string> order) : IRollingDeductionAdjustmentStore
{
    public Dictionary<string, RollingDeductionAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingDeductionAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingDeductionAdjustment item)
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

    public List<RollingDeductionAdjustment> List()
    {
        var outValue = new List<RollingDeductionAdjustment>();
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
public sealed class CachedRollingDeductionAdjustmentStore(MemoryRollingDeductionAdjustmentStore inner, RollingDeductionAdjustmentCache cache, int hits, int misses) : IRollingDeductionAdjustmentStore
{
    public MemoryRollingDeductionAdjustmentStore Inner { get; set; } = inner;
    public RollingDeductionAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingDeductionAdjustment? Find(string code)
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

    public void Save(RollingDeductionAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingDeductionAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling deduction adjustments. Empty fields match all.</summary>
public sealed record RollingDeductionAdjustmentQuery(string Unit, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(RollingDeductionAdjustment item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingDeductionAdjustment> Apply(List<RollingDeductionAdjustment> items)
    {
        var matched = Collections.Filter(items, (RollingDeductionAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling deduction adjustment store.</summary>
public static class RollingDeductionAdjustmentStore
{
    public static IRollingDeductionAdjustmentStore NewMemoryRollingDeductionAdjustmentStore()
    {
        return new MemoryRollingDeductionAdjustmentStore(
            new Dictionary<string, RollingDeductionAdjustment>(),
            new List<string>());
    }

    public static IRollingDeductionAdjustmentStore NewCachedRollingDeductionAdjustmentStore(MemoryRollingDeductionAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedRollingDeductionAdjustmentStore(
            inner,
            RollingDeductionAdjustmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RollingDeductionAdjustment> FindRollingDeductionAdjustments(IRollingDeductionAdjustmentStore store, RollingDeductionAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
