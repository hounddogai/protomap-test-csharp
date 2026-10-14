namespace Acme.Workforce.Orgdesign.Position.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position ledgers by code.</summary>
public interface IPositionLedgerStore
{
    PositionLedger? Find(string code);
    void Save(PositionLedger item);
    bool Remove(string code);
    List<PositionLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionLedgerStore(Dictionary<string, PositionLedger> items, List<string> order) : IPositionLedgerStore
{
    public Dictionary<string, PositionLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionLedger item)
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

    public List<PositionLedger> List()
    {
        var outValue = new List<PositionLedger>();
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
public sealed class CachedPositionLedgerStore(MemoryPositionLedgerStore inner, PositionLedgerCache cache, int hits, int misses) : IPositionLedgerStore
{
    public MemoryPositionLedgerStore Inner { get; set; } = inner;
    public PositionLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionLedger? Find(string code)
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

    public void Save(PositionLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position ledgers. Empty fields match all.</summary>
public sealed record PositionLedgerQuery(string Region, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(PositionLedger item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PositionLedger> Apply(List<PositionLedger> items)
    {
        var matched = Collections.Filter(items, (PositionLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position ledger store.</summary>
public static class PositionLedgerStore
{
    public static IPositionLedgerStore NewMemoryPositionLedgerStore()
    {
        return new MemoryPositionLedgerStore(new Dictionary<string, PositionLedger>(), new List<string>());
    }

    public static IPositionLedgerStore NewCachedPositionLedgerStore(MemoryPositionLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedPositionLedgerStore(inner, PositionLedgerCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PositionLedger> FindPositionLedgers(IPositionLedgerStore store, PositionLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
