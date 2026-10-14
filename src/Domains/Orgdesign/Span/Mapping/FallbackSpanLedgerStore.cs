namespace Acme.Workforce.Orgdesign.Span.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback span ledgers by code.</summary>
public interface IFallbackSpanLedgerStore
{
    FallbackSpanLedger? Find(string code);
    void Save(FallbackSpanLedger item);
    bool Remove(string code);
    List<FallbackSpanLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackSpanLedgerStore(Dictionary<string, FallbackSpanLedger> items, List<string> order) : IFallbackSpanLedgerStore
{
    public Dictionary<string, FallbackSpanLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackSpanLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackSpanLedger item)
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

    public List<FallbackSpanLedger> List()
    {
        var outValue = new List<FallbackSpanLedger>();
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
public sealed class CachedFallbackSpanLedgerStore(MemoryFallbackSpanLedgerStore inner, FallbackSpanLedgerCache cache, int hits, int misses) : IFallbackSpanLedgerStore
{
    public MemoryFallbackSpanLedgerStore Inner { get; set; } = inner;
    public FallbackSpanLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackSpanLedger? Find(string code)
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

    public void Save(FallbackSpanLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackSpanLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback span ledgers. Empty fields match all.</summary>
public sealed record FallbackSpanLedgerQuery(string Name, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackSpanLedger item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<FallbackSpanLedger> Apply(List<FallbackSpanLedger> items)
    {
        var matched = Collections.Filter(items, (FallbackSpanLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback span ledger store.</summary>
public static class FallbackSpanLedgerStore
{
    public static IFallbackSpanLedgerStore NewMemoryFallbackSpanLedgerStore()
    {
        return new MemoryFallbackSpanLedgerStore(new Dictionary<string, FallbackSpanLedger>(), new List<string>());
    }

    public static IFallbackSpanLedgerStore NewCachedFallbackSpanLedgerStore(MemoryFallbackSpanLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackSpanLedgerStore(inner, FallbackSpanLedgerCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<FallbackSpanLedger> FindFallbackSpanLedgers(IFallbackSpanLedgerStore store, FallbackSpanLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
