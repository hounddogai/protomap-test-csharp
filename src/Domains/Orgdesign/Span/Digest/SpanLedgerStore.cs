namespace Acme.Workforce.Orgdesign.Span.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span ledgers by code.</summary>
public interface ISpanLedgerStore
{
    SpanLedger? Find(string code);
    void Save(SpanLedger item);
    bool Remove(string code);
    List<SpanLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanLedgerStore(Dictionary<string, SpanLedger> items, List<string> order) : ISpanLedgerStore
{
    public Dictionary<string, SpanLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanLedger item)
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

    public List<SpanLedger> List()
    {
        var outValue = new List<SpanLedger>();
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
public sealed class CachedSpanLedgerStore(MemorySpanLedgerStore inner, SpanLedgerCache cache, int hits, int misses) : ISpanLedgerStore
{
    public MemorySpanLedgerStore Inner { get; set; } = inner;
    public SpanLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanLedger? Find(string code)
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

    public void Save(SpanLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span ledgers. Empty fields match all.</summary>
public sealed record SpanLedgerQuery(string Segment, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(SpanLedger item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanLedger> Apply(List<SpanLedger> items)
    {
        var matched = Collections.Filter(items, (SpanLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span ledger store.</summary>
public static class SpanLedgerStore
{
    public static ISpanLedgerStore NewMemorySpanLedgerStore()
    {
        return new MemorySpanLedgerStore(new Dictionary<string, SpanLedger>(), new List<string>());
    }

    public static ISpanLedgerStore NewCachedSpanLedgerStore(MemorySpanLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedSpanLedgerStore(inner, SpanLedgerCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SpanLedger> FindSpanLedgers(ISpanLedgerStore store, SpanLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
