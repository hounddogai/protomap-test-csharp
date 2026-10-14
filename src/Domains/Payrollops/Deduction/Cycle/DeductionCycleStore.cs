namespace Acme.Workforce.Payrollops.Deduction.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction cycles by code.</summary>
public interface IDeductionCycleStore
{
    DeductionCycle? Find(string code);
    void Save(DeductionCycle item);
    bool Remove(string code);
    List<DeductionCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionCycleStore(Dictionary<string, DeductionCycle> items, List<string> order) : IDeductionCycleStore
{
    public Dictionary<string, DeductionCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionCycle item)
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

    public List<DeductionCycle> List()
    {
        var outValue = new List<DeductionCycle>();
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
public sealed class CachedDeductionCycleStore(MemoryDeductionCycleStore inner, DeductionCycleCache cache, int hits, int misses) : IDeductionCycleStore
{
    public MemoryDeductionCycleStore Inner { get; set; } = inner;
    public DeductionCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionCycle? Find(string code)
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

    public void Save(DeductionCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction cycles. Empty fields match all.</summary>
public sealed record DeductionCycleQuery(string Department, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionCycle item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeductionCycle> Apply(List<DeductionCycle> items)
    {
        var matched = Collections.Filter(items, (DeductionCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction cycle store.</summary>
public static class DeductionCycleStore
{
    public static IDeductionCycleStore NewMemoryDeductionCycleStore()
    {
        return new MemoryDeductionCycleStore(new Dictionary<string, DeductionCycle>(), new List<string>());
    }

    public static IDeductionCycleStore NewCachedDeductionCycleStore(MemoryDeductionCycleStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionCycleStore(inner, DeductionCycleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<DeductionCycle> FindDeductionCycles(IDeductionCycleStore store, DeductionCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
