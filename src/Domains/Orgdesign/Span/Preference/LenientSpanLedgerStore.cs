namespace Acme.Workforce.Orgdesign.Span.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient span ledgers by code.</summary>
public interface ILenientSpanLedgerStore
{
    LenientSpanLedger? Find(string code);
    void Save(LenientSpanLedger item);
    bool Remove(string code);
    List<LenientSpanLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientSpanLedgerStore(Dictionary<string, LenientSpanLedger> items, List<string> order) : ILenientSpanLedgerStore
{
    public Dictionary<string, LenientSpanLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientSpanLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientSpanLedger item)
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

    public List<LenientSpanLedger> List()
    {
        var outValue = new List<LenientSpanLedger>();
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
public sealed class CachedLenientSpanLedgerStore(MemoryLenientSpanLedgerStore inner, LenientSpanLedgerCache cache, int hits, int misses) : ILenientSpanLedgerStore
{
    public MemoryLenientSpanLedgerStore Inner { get; set; } = inner;
    public LenientSpanLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientSpanLedger? Find(string code)
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

    public void Save(LenientSpanLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientSpanLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient span ledgers. Empty fields match all.</summary>
public sealed record LenientSpanLedgerQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(LenientSpanLedger item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientSpanLedger> Apply(List<LenientSpanLedger> items)
    {
        var matched = Collections.Filter(items, (LenientSpanLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient span ledger store.</summary>
public static class LenientSpanLedgerStore
{
    public static ILenientSpanLedgerStore NewMemoryLenientSpanLedgerStore()
    {
        return new MemoryLenientSpanLedgerStore(new Dictionary<string, LenientSpanLedger>(), new List<string>());
    }

    public static ILenientSpanLedgerStore NewCachedLenientSpanLedgerStore(MemoryLenientSpanLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedLenientSpanLedgerStore(inner, LenientSpanLedgerCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LenientSpanLedger> FindLenientSpanLedgers(ILenientSpanLedgerStore store, LenientSpanLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
