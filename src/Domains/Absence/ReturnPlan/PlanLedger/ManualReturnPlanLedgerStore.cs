namespace Acme.Workforce.Absence.ReturnPlan.PlanLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual return plan ledgers by code.</summary>
public interface IManualReturnPlanLedgerStore
{
    ManualReturnPlanLedger? Find(string code);
    void Save(ManualReturnPlanLedger item);
    bool Remove(string code);
    List<ManualReturnPlanLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualReturnPlanLedgerStore(Dictionary<string, ManualReturnPlanLedger> items, List<string> order) : IManualReturnPlanLedgerStore
{
    public Dictionary<string, ManualReturnPlanLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualReturnPlanLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualReturnPlanLedger item)
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

    public List<ManualReturnPlanLedger> List()
    {
        var outValue = new List<ManualReturnPlanLedger>();
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
public sealed class CachedManualReturnPlanLedgerStore(MemoryManualReturnPlanLedgerStore inner, ManualReturnPlanLedgerCache cache, int hits, int misses) : IManualReturnPlanLedgerStore
{
    public MemoryManualReturnPlanLedgerStore Inner { get; set; } = inner;
    public ManualReturnPlanLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualReturnPlanLedger? Find(string code)
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

    public void Save(ManualReturnPlanLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualReturnPlanLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual return plan ledgers. Empty fields match all.</summary>
public sealed record ManualReturnPlanLedgerQuery(string Label, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(ManualReturnPlanLedger item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<ManualReturnPlanLedger> Apply(List<ManualReturnPlanLedger> items)
    {
        var matched = Collections.Filter(items, (ManualReturnPlanLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual return plan ledger store.</summary>
public static class ManualReturnPlanLedgerStore
{
    public static IManualReturnPlanLedgerStore NewMemoryManualReturnPlanLedgerStore()
    {
        return new MemoryManualReturnPlanLedgerStore(
            new Dictionary<string, ManualReturnPlanLedger>(),
            new List<string>());
    }

    public static IManualReturnPlanLedgerStore NewCachedManualReturnPlanLedgerStore(MemoryManualReturnPlanLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedManualReturnPlanLedgerStore(
            inner,
            ManualReturnPlanLedgerCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<ManualReturnPlanLedger> FindManualReturnPlanLedgers(IManualReturnPlanLedgerStore store, ManualReturnPlanLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
