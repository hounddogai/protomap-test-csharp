namespace Acme.Workforce.Orgdesign.Span.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span records by code.</summary>
public interface ISpanRecordStore
{
    SpanRecord? Find(string code);
    void Save(SpanRecord item);
    bool Remove(string code);
    List<SpanRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanRecordStore(Dictionary<string, SpanRecord> items, List<string> order) : ISpanRecordStore
{
    public Dictionary<string, SpanRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanRecord item)
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

    public List<SpanRecord> List()
    {
        var outValue = new List<SpanRecord>();
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
public sealed class CachedSpanRecordStore(MemorySpanRecordStore inner, SpanRecordCache cache, int hits, int misses) : ISpanRecordStore
{
    public MemorySpanRecordStore Inner { get; set; } = inner;
    public SpanRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanRecord? Find(string code)
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

    public void Save(SpanRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span records. Empty fields match all.</summary>
public sealed record SpanRecordQuery(string Category, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(SpanRecord item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanRecord> Apply(List<SpanRecord> items)
    {
        var matched = Collections.Filter(items, (SpanRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span record store.</summary>
public static class SpanRecordStore
{
    public static ISpanRecordStore NewMemorySpanRecordStore()
    {
        return new MemorySpanRecordStore(new Dictionary<string, SpanRecord>(), new List<string>());
    }

    public static ISpanRecordStore NewCachedSpanRecordStore(MemorySpanRecordStore inner, int ttl, IClock clock)
    {
        return new CachedSpanRecordStore(inner, SpanRecordCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SpanRecord> FindSpanRecords(ISpanRecordStore store, SpanRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
