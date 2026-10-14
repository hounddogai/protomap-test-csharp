namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan ledgers by code.</summary>
public interface IActionPlanLedgerStore
{
    ActionPlanLedger? Find(string code);
    void Save(ActionPlanLedger item);
    bool Remove(string code);
    List<ActionPlanLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanLedgerStore(Dictionary<string, ActionPlanLedger> items, List<string> order) : IActionPlanLedgerStore
{
    public Dictionary<string, ActionPlanLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanLedger item)
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

    public List<ActionPlanLedger> List()
    {
        var outValue = new List<ActionPlanLedger>();
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
public sealed class CachedActionPlanLedgerStore(MemoryActionPlanLedgerStore inner, ActionPlanLedgerCache cache, int hits, int misses) : IActionPlanLedgerStore
{
    public MemoryActionPlanLedgerStore Inner { get; set; } = inner;
    public ActionPlanLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanLedger? Find(string code)
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

    public void Save(ActionPlanLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan ledgers. Empty fields match all.</summary>
public sealed record ActionPlanLedgerQuery(string Team, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanLedger item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ActionPlanLedger> Apply(List<ActionPlanLedger> items)
    {
        var matched = Collections.Filter(items, (ActionPlanLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan ledger store.</summary>
public static class ActionPlanLedgerStore
{
    public static IActionPlanLedgerStore NewMemoryActionPlanLedgerStore()
    {
        return new MemoryActionPlanLedgerStore(new Dictionary<string, ActionPlanLedger>(), new List<string>());
    }

    public static IActionPlanLedgerStore NewCachedActionPlanLedgerStore(MemoryActionPlanLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanLedgerStore(inner, ActionPlanLedgerCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ActionPlanLedger> FindActionPlanLedgers(IActionPlanLedgerStore store, ActionPlanLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
