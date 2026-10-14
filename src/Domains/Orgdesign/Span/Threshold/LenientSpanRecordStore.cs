namespace Acme.Workforce.Orgdesign.Span.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient span records by code.</summary>
public interface ILenientSpanRecordStore
{
    LenientSpanRecord? Find(string code);
    void Save(LenientSpanRecord item);
    bool Remove(string code);
    List<LenientSpanRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientSpanRecordStore(Dictionary<string, LenientSpanRecord> items, List<string> order) : ILenientSpanRecordStore
{
    public Dictionary<string, LenientSpanRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientSpanRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientSpanRecord item)
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

    public List<LenientSpanRecord> List()
    {
        var outValue = new List<LenientSpanRecord>();
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
public sealed class CachedLenientSpanRecordStore(MemoryLenientSpanRecordStore inner, LenientSpanRecordCache cache, int hits, int misses) : ILenientSpanRecordStore
{
    public MemoryLenientSpanRecordStore Inner { get; set; } = inner;
    public LenientSpanRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientSpanRecord? Find(string code)
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

    public void Save(LenientSpanRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientSpanRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient span records. Empty fields match all.</summary>
public sealed record LenientSpanRecordQuery(string Source, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(LenientSpanRecord item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientSpanRecord> Apply(List<LenientSpanRecord> items)
    {
        var matched = Collections.Filter(items, (LenientSpanRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient span record store.</summary>
public static class LenientSpanRecordStore
{
    public static ILenientSpanRecordStore NewMemoryLenientSpanRecordStore()
    {
        return new MemoryLenientSpanRecordStore(new Dictionary<string, LenientSpanRecord>(), new List<string>());
    }

    public static ILenientSpanRecordStore NewCachedLenientSpanRecordStore(MemoryLenientSpanRecordStore inner, int ttl, IClock clock)
    {
        return new CachedLenientSpanRecordStore(inner, LenientSpanRecordCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LenientSpanRecord> FindLenientSpanRecords(ILenientSpanRecordStore store, LenientSpanRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
