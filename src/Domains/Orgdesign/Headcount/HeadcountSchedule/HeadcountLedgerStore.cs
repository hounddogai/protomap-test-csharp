namespace Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount ledgers by code.</summary>
public interface IHeadcountLedgerStore
{
    HeadcountLedger? Find(string code);
    void Save(HeadcountLedger item);
    bool Remove(string code);
    List<HeadcountLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountLedgerStore(Dictionary<string, HeadcountLedger> items, List<string> order) : IHeadcountLedgerStore
{
    public Dictionary<string, HeadcountLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountLedger item)
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

    public List<HeadcountLedger> List()
    {
        var outValue = new List<HeadcountLedger>();
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
public sealed class CachedHeadcountLedgerStore(MemoryHeadcountLedgerStore inner, HeadcountLedgerCache cache, int hits, int misses) : IHeadcountLedgerStore
{
    public MemoryHeadcountLedgerStore Inner { get; set; } = inner;
    public HeadcountLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountLedger? Find(string code)
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

    public void Save(HeadcountLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount ledgers. Empty fields match all.</summary>
public sealed record HeadcountLedgerQuery(string Department, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountLedger item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<HeadcountLedger> Apply(List<HeadcountLedger> items)
    {
        var matched = Collections.Filter(items, (HeadcountLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount ledger store.</summary>
public static class HeadcountLedgerStore
{
    public static IHeadcountLedgerStore NewMemoryHeadcountLedgerStore()
    {
        return new MemoryHeadcountLedgerStore(new Dictionary<string, HeadcountLedger>(), new List<string>());
    }

    public static IHeadcountLedgerStore NewCachedHeadcountLedgerStore(MemoryHeadcountLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountLedgerStore(inner, HeadcountLedgerCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<HeadcountLedger> FindHeadcountLedgers(IHeadcountLedgerStore store, HeadcountLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
