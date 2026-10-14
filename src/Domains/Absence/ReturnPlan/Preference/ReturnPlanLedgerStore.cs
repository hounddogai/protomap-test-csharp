namespace Acme.Workforce.Absence.ReturnPlan.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan ledgers by code.</summary>
public interface IReturnPlanLedgerStore
{
    ReturnPlanLedger? Find(string code);
    void Save(ReturnPlanLedger item);
    bool Remove(string code);
    List<ReturnPlanLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanLedgerStore(Dictionary<string, ReturnPlanLedger> items, List<string> order) : IReturnPlanLedgerStore
{
    public Dictionary<string, ReturnPlanLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanLedger item)
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

    public List<ReturnPlanLedger> List()
    {
        var outValue = new List<ReturnPlanLedger>();
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
public sealed class CachedReturnPlanLedgerStore(MemoryReturnPlanLedgerStore inner, ReturnPlanLedgerCache cache, int hits, int misses) : IReturnPlanLedgerStore
{
    public MemoryReturnPlanLedgerStore Inner { get; set; } = inner;
    public ReturnPlanLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanLedger? Find(string code)
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

    public void Save(ReturnPlanLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan ledgers. Empty fields match all.</summary>
public sealed record ReturnPlanLedgerQuery(string Unit, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanLedger item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReturnPlanLedger> Apply(List<ReturnPlanLedger> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan ledger store.</summary>
public static class ReturnPlanLedgerStore
{
    public static IReturnPlanLedgerStore NewMemoryReturnPlanLedgerStore()
    {
        return new MemoryReturnPlanLedgerStore(new Dictionary<string, ReturnPlanLedger>(), new List<string>());
    }

    public static IReturnPlanLedgerStore NewCachedReturnPlanLedgerStore(MemoryReturnPlanLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanLedgerStore(inner, ReturnPlanLedgerCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanLedger> FindReturnPlanLedgers(IReturnPlanLedgerStore store, ReturnPlanLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
