namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan ledgers by code.</summary>
public interface ISuccessionPlanLedgerStore
{
    SuccessionPlanLedger? Find(string code);
    void Save(SuccessionPlanLedger item);
    bool Remove(string code);
    List<SuccessionPlanLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanLedgerStore(Dictionary<string, SuccessionPlanLedger> items, List<string> order) : ISuccessionPlanLedgerStore
{
    public Dictionary<string, SuccessionPlanLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanLedger item)
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

    public List<SuccessionPlanLedger> List()
    {
        var outValue = new List<SuccessionPlanLedger>();
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
public sealed class CachedSuccessionPlanLedgerStore(MemorySuccessionPlanLedgerStore inner, SuccessionPlanLedgerCache cache, int hits, int misses) : ISuccessionPlanLedgerStore
{
    public MemorySuccessionPlanLedgerStore Inner { get; set; } = inner;
    public SuccessionPlanLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanLedger? Find(string code)
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

    public void Save(SuccessionPlanLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan ledgers. Empty fields match all.</summary>
public sealed record SuccessionPlanLedgerQuery(string Locale, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanLedger item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SuccessionPlanLedger> Apply(List<SuccessionPlanLedger> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan ledger store.</summary>
public static class SuccessionPlanLedgerStore
{
    public static ISuccessionPlanLedgerStore NewMemorySuccessionPlanLedgerStore()
    {
        return new MemorySuccessionPlanLedgerStore(
            new Dictionary<string, SuccessionPlanLedger>(),
            new List<string>());
    }

    public static ISuccessionPlanLedgerStore NewCachedSuccessionPlanLedgerStore(MemorySuccessionPlanLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanLedgerStore(inner, SuccessionPlanLedgerCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SuccessionPlanLedger> FindSuccessionPlanLedgers(ISuccessionPlanLedgerStore store, SuccessionPlanLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
