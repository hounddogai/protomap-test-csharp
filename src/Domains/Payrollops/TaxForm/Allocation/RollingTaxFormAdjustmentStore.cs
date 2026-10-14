namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling tax form adjustments by code.</summary>
public interface IRollingTaxFormAdjustmentStore
{
    RollingTaxFormAdjustment? Find(string code);
    void Save(RollingTaxFormAdjustment item);
    bool Remove(string code);
    List<RollingTaxFormAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingTaxFormAdjustmentStore(Dictionary<string, RollingTaxFormAdjustment> items, List<string> order) : IRollingTaxFormAdjustmentStore
{
    public Dictionary<string, RollingTaxFormAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingTaxFormAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingTaxFormAdjustment item)
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

    public List<RollingTaxFormAdjustment> List()
    {
        var outValue = new List<RollingTaxFormAdjustment>();
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
public sealed class CachedRollingTaxFormAdjustmentStore(MemoryRollingTaxFormAdjustmentStore inner, RollingTaxFormAdjustmentCache cache, int hits, int misses) : IRollingTaxFormAdjustmentStore
{
    public MemoryRollingTaxFormAdjustmentStore Inner { get; set; } = inner;
    public RollingTaxFormAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingTaxFormAdjustment? Find(string code)
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

    public void Save(RollingTaxFormAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingTaxFormAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling tax form adjustments. Empty fields match all.</summary>
public sealed record RollingTaxFormAdjustmentQuery(string Region, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(RollingTaxFormAdjustment item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingTaxFormAdjustment> Apply(List<RollingTaxFormAdjustment> items)
    {
        var matched = Collections.Filter(items, (RollingTaxFormAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling tax form adjustment store.</summary>
public static class RollingTaxFormAdjustmentStore
{
    public static IRollingTaxFormAdjustmentStore NewMemoryRollingTaxFormAdjustmentStore()
    {
        return new MemoryRollingTaxFormAdjustmentStore(
            new Dictionary<string, RollingTaxFormAdjustment>(),
            new List<string>());
    }

    public static IRollingTaxFormAdjustmentStore NewCachedRollingTaxFormAdjustmentStore(MemoryRollingTaxFormAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedRollingTaxFormAdjustmentStore(
            inner,
            RollingTaxFormAdjustmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<RollingTaxFormAdjustment> FindRollingTaxFormAdjustments(IRollingTaxFormAdjustmentStore store, RollingTaxFormAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
