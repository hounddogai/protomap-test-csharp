namespace Acme.Workforce.Assets.Laptop.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop ledgers by code.</summary>
public interface ILaptopLedgerStore
{
    LaptopLedger? Find(string code);
    void Save(LaptopLedger item);
    bool Remove(string code);
    List<LaptopLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopLedgerStore(Dictionary<string, LaptopLedger> items, List<string> order) : ILaptopLedgerStore
{
    public Dictionary<string, LaptopLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopLedger item)
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

    public List<LaptopLedger> List()
    {
        var outValue = new List<LaptopLedger>();
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
public sealed class CachedLaptopLedgerStore(MemoryLaptopLedgerStore inner, LaptopLedgerCache cache, int hits, int misses) : ILaptopLedgerStore
{
    public MemoryLaptopLedgerStore Inner { get; set; } = inner;
    public LaptopLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopLedger? Find(string code)
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

    public void Save(LaptopLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop ledgers. Empty fields match all.</summary>
public sealed record LaptopLedgerQuery(string Label, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopLedger item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopLedger> Apply(List<LaptopLedger> items)
    {
        var matched = Collections.Filter(items, (LaptopLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop ledger store.</summary>
public static class LaptopLedgerStore
{
    public static ILaptopLedgerStore NewMemoryLaptopLedgerStore()
    {
        return new MemoryLaptopLedgerStore(new Dictionary<string, LaptopLedger>(), new List<string>());
    }

    public static ILaptopLedgerStore NewCachedLaptopLedgerStore(MemoryLaptopLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopLedgerStore(inner, LaptopLedgerCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LaptopLedger> FindLaptopLedgers(ILaptopLedgerStore store, LaptopLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
