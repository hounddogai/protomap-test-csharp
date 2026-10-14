namespace Acme.Workforce.Payrollops.PayRun.RunLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling pay run cycles by code.</summary>
public interface IRollingPayRunCycleStore
{
    RollingPayRunCycle? Find(string code);
    void Save(RollingPayRunCycle item);
    bool Remove(string code);
    List<RollingPayRunCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingPayRunCycleStore(Dictionary<string, RollingPayRunCycle> items, List<string> order) : IRollingPayRunCycleStore
{
    public Dictionary<string, RollingPayRunCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingPayRunCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingPayRunCycle item)
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

    public List<RollingPayRunCycle> List()
    {
        var outValue = new List<RollingPayRunCycle>();
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
public sealed class CachedRollingPayRunCycleStore(MemoryRollingPayRunCycleStore inner, RollingPayRunCycleCache cache, int hits, int misses) : IRollingPayRunCycleStore
{
    public MemoryRollingPayRunCycleStore Inner { get; set; } = inner;
    public RollingPayRunCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingPayRunCycle? Find(string code)
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

    public void Save(RollingPayRunCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingPayRunCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling pay run cycles. Empty fields match all.</summary>
public sealed record RollingPayRunCycleQuery(string Label, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(RollingPayRunCycle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingPayRunCycle> Apply(List<RollingPayRunCycle> items)
    {
        var matched = Collections.Filter(items, (RollingPayRunCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling pay run cycle store.</summary>
public static class RollingPayRunCycleStore
{
    public static IRollingPayRunCycleStore NewMemoryRollingPayRunCycleStore()
    {
        return new MemoryRollingPayRunCycleStore(new Dictionary<string, RollingPayRunCycle>(), new List<string>());
    }

    public static IRollingPayRunCycleStore NewCachedRollingPayRunCycleStore(MemoryRollingPayRunCycleStore inner, int ttl, IClock clock)
    {
        return new CachedRollingPayRunCycleStore(inner, RollingPayRunCycleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RollingPayRunCycle> FindRollingPayRunCycles(IRollingPayRunCycleStore store, RollingPayRunCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
