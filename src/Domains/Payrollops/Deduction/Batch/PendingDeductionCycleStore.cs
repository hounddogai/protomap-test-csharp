namespace Acme.Workforce.Payrollops.Deduction.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending deduction cycles by code.</summary>
public interface IPendingDeductionCycleStore
{
    PendingDeductionCycle? Find(string code);
    void Save(PendingDeductionCycle item);
    bool Remove(string code);
    List<PendingDeductionCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingDeductionCycleStore(Dictionary<string, PendingDeductionCycle> items, List<string> order) : IPendingDeductionCycleStore
{
    public Dictionary<string, PendingDeductionCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingDeductionCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingDeductionCycle item)
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

    public List<PendingDeductionCycle> List()
    {
        var outValue = new List<PendingDeductionCycle>();
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
public sealed class CachedPendingDeductionCycleStore(MemoryPendingDeductionCycleStore inner, PendingDeductionCycleCache cache, int hits, int misses) : IPendingDeductionCycleStore
{
    public MemoryPendingDeductionCycleStore Inner { get; set; } = inner;
    public PendingDeductionCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingDeductionCycle? Find(string code)
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

    public void Save(PendingDeductionCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingDeductionCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending deduction cycles. Empty fields match all.</summary>
public sealed record PendingDeductionCycleQuery(string Label, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(PendingDeductionCycle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingDeductionCycle> Apply(List<PendingDeductionCycle> items)
    {
        var matched = Collections.Filter(items, (PendingDeductionCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending deduction cycle store.</summary>
public static class PendingDeductionCycleStore
{
    public static IPendingDeductionCycleStore NewMemoryPendingDeductionCycleStore()
    {
        return new MemoryPendingDeductionCycleStore(
            new Dictionary<string, PendingDeductionCycle>(),
            new List<string>());
    }

    public static IPendingDeductionCycleStore NewCachedPendingDeductionCycleStore(MemoryPendingDeductionCycleStore inner, int ttl, IClock clock)
    {
        return new CachedPendingDeductionCycleStore(
            inner,
            PendingDeductionCycleCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PendingDeductionCycle> FindPendingDeductionCycles(IPendingDeductionCycleStore store, PendingDeductionCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
