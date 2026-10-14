namespace Acme.Workforce.Payrollops.Garnishment.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment ledgers by code.</summary>
public interface IGarnishmentLedgerStore
{
    GarnishmentLedger? Find(string code);
    void Save(GarnishmentLedger item);
    bool Remove(string code);
    List<GarnishmentLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentLedgerStore(Dictionary<string, GarnishmentLedger> items, List<string> order) : IGarnishmentLedgerStore
{
    public Dictionary<string, GarnishmentLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentLedger item)
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

    public List<GarnishmentLedger> List()
    {
        var outValue = new List<GarnishmentLedger>();
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
public sealed class CachedGarnishmentLedgerStore(MemoryGarnishmentLedgerStore inner, GarnishmentLedgerCache cache, int hits, int misses) : IGarnishmentLedgerStore
{
    public MemoryGarnishmentLedgerStore Inner { get; set; } = inner;
    public GarnishmentLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentLedger? Find(string code)
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

    public void Save(GarnishmentLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment ledgers. Empty fields match all.</summary>
public sealed record GarnishmentLedgerQuery(string Unit, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentLedger item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<GarnishmentLedger> Apply(List<GarnishmentLedger> items)
    {
        var matched = Collections.Filter(items, (GarnishmentLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment ledger store.</summary>
public static class GarnishmentLedgerStore
{
    public static IGarnishmentLedgerStore NewMemoryGarnishmentLedgerStore()
    {
        return new MemoryGarnishmentLedgerStore(new Dictionary<string, GarnishmentLedger>(), new List<string>());
    }

    public static IGarnishmentLedgerStore NewCachedGarnishmentLedgerStore(MemoryGarnishmentLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentLedgerStore(inner, GarnishmentLedgerCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<GarnishmentLedger> FindGarnishmentLedgers(IGarnishmentLedgerStore store, GarnishmentLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
