namespace Acme.Workforce.Workplace.FloorPlan.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps floor plan ledgers by code.</summary>
public interface IFloorPlanLedgerStore
{
    FloorPlanLedger? Find(string code);
    void Save(FloorPlanLedger item);
    bool Remove(string code);
    List<FloorPlanLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFloorPlanLedgerStore(Dictionary<string, FloorPlanLedger> items, List<string> order) : IFloorPlanLedgerStore
{
    public Dictionary<string, FloorPlanLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FloorPlanLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FloorPlanLedger item)
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

    public List<FloorPlanLedger> List()
    {
        var outValue = new List<FloorPlanLedger>();
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
public sealed class CachedFloorPlanLedgerStore(MemoryFloorPlanLedgerStore inner, FloorPlanLedgerCache cache, int hits, int misses) : IFloorPlanLedgerStore
{
    public MemoryFloorPlanLedgerStore Inner { get; set; } = inner;
    public FloorPlanLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FloorPlanLedger? Find(string code)
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

    public void Save(FloorPlanLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FloorPlanLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects floor plan ledgers. Empty fields match all.</summary>
public sealed record FloorPlanLedgerQuery(string Unit, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(FloorPlanLedger item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FloorPlanLedger> Apply(List<FloorPlanLedger> items)
    {
        var matched = Collections.Filter(items, (FloorPlanLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of floor plan ledger store.</summary>
public static class FloorPlanLedgerStore
{
    public static IFloorPlanLedgerStore NewMemoryFloorPlanLedgerStore()
    {
        return new MemoryFloorPlanLedgerStore(new Dictionary<string, FloorPlanLedger>(), new List<string>());
    }

    public static IFloorPlanLedgerStore NewCachedFloorPlanLedgerStore(MemoryFloorPlanLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedFloorPlanLedgerStore(inner, FloorPlanLedgerCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<FloorPlanLedger> FindFloorPlanLedgers(IFloorPlanLedgerStore store, FloorPlanLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
