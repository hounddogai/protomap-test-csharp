namespace Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary action plan ledgers by code.</summary>
public interface IPrimaryActionPlanLedgerStore
{
    PrimaryActionPlanLedger? Find(string code);
    void Save(PrimaryActionPlanLedger item);
    bool Remove(string code);
    List<PrimaryActionPlanLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryActionPlanLedgerStore(Dictionary<string, PrimaryActionPlanLedger> items, List<string> order) : IPrimaryActionPlanLedgerStore
{
    public Dictionary<string, PrimaryActionPlanLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryActionPlanLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryActionPlanLedger item)
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

    public List<PrimaryActionPlanLedger> List()
    {
        var outValue = new List<PrimaryActionPlanLedger>();
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
public sealed class CachedPrimaryActionPlanLedgerStore(MemoryPrimaryActionPlanLedgerStore inner, PrimaryActionPlanLedgerCache cache, int hits, int misses) : IPrimaryActionPlanLedgerStore
{
    public MemoryPrimaryActionPlanLedgerStore Inner { get; set; } = inner;
    public PrimaryActionPlanLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryActionPlanLedger? Find(string code)
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

    public void Save(PrimaryActionPlanLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryActionPlanLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary action plan ledgers. Empty fields match all.</summary>
public sealed record PrimaryActionPlanLedgerQuery(string Region, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryActionPlanLedger item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryActionPlanLedger> Apply(List<PrimaryActionPlanLedger> items)
    {
        var matched = Collections.Filter(items, (PrimaryActionPlanLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary action plan ledger store.</summary>
public static class PrimaryActionPlanLedgerStore
{
    public static IPrimaryActionPlanLedgerStore NewMemoryPrimaryActionPlanLedgerStore()
    {
        return new MemoryPrimaryActionPlanLedgerStore(
            new Dictionary<string, PrimaryActionPlanLedger>(),
            new List<string>());
    }

    public static IPrimaryActionPlanLedgerStore NewCachedPrimaryActionPlanLedgerStore(MemoryPrimaryActionPlanLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryActionPlanLedgerStore(
            inner,
            PrimaryActionPlanLedgerCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryActionPlanLedger> FindPrimaryActionPlanLedgers(IPrimaryActionPlanLedgerStore store, PrimaryActionPlanLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
