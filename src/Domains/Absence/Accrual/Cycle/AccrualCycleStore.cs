namespace Acme.Workforce.Absence.Accrual.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual cycles by code.</summary>
public interface IAccrualCycleStore
{
    AccrualCycle? Find(string code);
    void Save(AccrualCycle item);
    bool Remove(string code);
    List<AccrualCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualCycleStore(Dictionary<string, AccrualCycle> items, List<string> order) : IAccrualCycleStore
{
    public Dictionary<string, AccrualCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualCycle item)
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

    public List<AccrualCycle> List()
    {
        var outValue = new List<AccrualCycle>();
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
public sealed class CachedAccrualCycleStore(MemoryAccrualCycleStore inner, AccrualCycleCache cache, int hits, int misses) : IAccrualCycleStore
{
    public MemoryAccrualCycleStore Inner { get; set; } = inner;
    public AccrualCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualCycle? Find(string code)
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

    public void Save(AccrualCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual cycles. Empty fields match all.</summary>
public sealed record AccrualCycleQuery(string Category, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualCycle item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccrualCycle> Apply(List<AccrualCycle> items)
    {
        var matched = Collections.Filter(items, (AccrualCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual cycle store.</summary>
public static class AccrualCycleStore
{
    public static IAccrualCycleStore NewMemoryAccrualCycleStore()
    {
        return new MemoryAccrualCycleStore(new Dictionary<string, AccrualCycle>(), new List<string>());
    }

    public static IAccrualCycleStore NewCachedAccrualCycleStore(MemoryAccrualCycleStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualCycleStore(inner, AccrualCycleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AccrualCycle> FindAccrualCycles(IAccrualCycleStore store, AccrualCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
