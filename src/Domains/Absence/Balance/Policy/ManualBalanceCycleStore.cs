namespace Acme.Workforce.Absence.Balance.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual balance cycles by code.</summary>
public interface IManualBalanceCycleStore
{
    ManualBalanceCycle? Find(string code);
    void Save(ManualBalanceCycle item);
    bool Remove(string code);
    List<ManualBalanceCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualBalanceCycleStore(Dictionary<string, ManualBalanceCycle> items, List<string> order) : IManualBalanceCycleStore
{
    public Dictionary<string, ManualBalanceCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualBalanceCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualBalanceCycle item)
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

    public List<ManualBalanceCycle> List()
    {
        var outValue = new List<ManualBalanceCycle>();
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
public sealed class CachedManualBalanceCycleStore(MemoryManualBalanceCycleStore inner, ManualBalanceCycleCache cache, int hits, int misses) : IManualBalanceCycleStore
{
    public MemoryManualBalanceCycleStore Inner { get; set; } = inner;
    public ManualBalanceCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualBalanceCycle? Find(string code)
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

    public void Save(ManualBalanceCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualBalanceCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual balance cycles. Empty fields match all.</summary>
public sealed record ManualBalanceCycleQuery(string Unit, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(ManualBalanceCycle item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualBalanceCycle> Apply(List<ManualBalanceCycle> items)
    {
        var matched = Collections.Filter(items, (ManualBalanceCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual balance cycle store.</summary>
public static class ManualBalanceCycleStore
{
    public static IManualBalanceCycleStore NewMemoryManualBalanceCycleStore()
    {
        return new MemoryManualBalanceCycleStore(new Dictionary<string, ManualBalanceCycle>(), new List<string>());
    }

    public static IManualBalanceCycleStore NewCachedManualBalanceCycleStore(MemoryManualBalanceCycleStore inner, int ttl, IClock clock)
    {
        return new CachedManualBalanceCycleStore(inner, ManualBalanceCycleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ManualBalanceCycle> FindManualBalanceCycles(IManualBalanceCycleStore store, ManualBalanceCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
