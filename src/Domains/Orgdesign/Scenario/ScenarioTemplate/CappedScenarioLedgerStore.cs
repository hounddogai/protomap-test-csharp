namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped scenario ledgers by code.</summary>
public interface ICappedScenarioLedgerStore
{
    CappedScenarioLedger? Find(string code);
    void Save(CappedScenarioLedger item);
    bool Remove(string code);
    List<CappedScenarioLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedScenarioLedgerStore(Dictionary<string, CappedScenarioLedger> items, List<string> order) : ICappedScenarioLedgerStore
{
    public Dictionary<string, CappedScenarioLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedScenarioLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedScenarioLedger item)
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

    public List<CappedScenarioLedger> List()
    {
        var outValue = new List<CappedScenarioLedger>();
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
public sealed class CachedCappedScenarioLedgerStore(MemoryCappedScenarioLedgerStore inner, CappedScenarioLedgerCache cache, int hits, int misses) : ICappedScenarioLedgerStore
{
    public MemoryCappedScenarioLedgerStore Inner { get; set; } = inner;
    public CappedScenarioLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedScenarioLedger? Find(string code)
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

    public void Save(CappedScenarioLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedScenarioLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped scenario ledgers. Empty fields match all.</summary>
public sealed record CappedScenarioLedgerQuery(string Country, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(CappedScenarioLedger item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedScenarioLedger> Apply(List<CappedScenarioLedger> items)
    {
        var matched = Collections.Filter(items, (CappedScenarioLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped scenario ledger store.</summary>
public static class CappedScenarioLedgerStore
{
    public static ICappedScenarioLedgerStore NewMemoryCappedScenarioLedgerStore()
    {
        return new MemoryCappedScenarioLedgerStore(
            new Dictionary<string, CappedScenarioLedger>(),
            new List<string>());
    }

    public static ICappedScenarioLedgerStore NewCachedCappedScenarioLedgerStore(MemoryCappedScenarioLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedCappedScenarioLedgerStore(inner, CappedScenarioLedgerCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<CappedScenarioLedger> FindCappedScenarioLedgers(ICappedScenarioLedgerStore store, CappedScenarioLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
