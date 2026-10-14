namespace Acme.Workforce.Orgdesign.CostModel.ModelManifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps cost model ledgers by code.</summary>
public interface ICostModelLedgerStore
{
    CostModelLedger? Find(string code);
    void Save(CostModelLedger item);
    bool Remove(string code);
    List<CostModelLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCostModelLedgerStore(Dictionary<string, CostModelLedger> items, List<string> order) : ICostModelLedgerStore
{
    public Dictionary<string, CostModelLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CostModelLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CostModelLedger item)
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

    public List<CostModelLedger> List()
    {
        var outValue = new List<CostModelLedger>();
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
public sealed class CachedCostModelLedgerStore(MemoryCostModelLedgerStore inner, CostModelLedgerCache cache, int hits, int misses) : ICostModelLedgerStore
{
    public MemoryCostModelLedgerStore Inner { get; set; } = inner;
    public CostModelLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CostModelLedger? Find(string code)
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

    public void Save(CostModelLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CostModelLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects cost model ledgers. Empty fields match all.</summary>
public sealed record CostModelLedgerQuery(string Label, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(CostModelLedger item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CostModelLedger> Apply(List<CostModelLedger> items)
    {
        var matched = Collections.Filter(items, (CostModelLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of cost model ledger store.</summary>
public static class CostModelLedgerStore
{
    public static ICostModelLedgerStore NewMemoryCostModelLedgerStore()
    {
        return new MemoryCostModelLedgerStore(new Dictionary<string, CostModelLedger>(), new List<string>());
    }

    public static ICostModelLedgerStore NewCachedCostModelLedgerStore(MemoryCostModelLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedCostModelLedgerStore(inner, CostModelLedgerCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<CostModelLedger> FindCostModelLedgers(ICostModelLedgerStore store, CostModelLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
