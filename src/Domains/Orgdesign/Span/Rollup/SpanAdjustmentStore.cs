namespace Acme.Workforce.Orgdesign.Span.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span adjustments by code.</summary>
public interface ISpanAdjustmentStore
{
    SpanAdjustment? Find(string code);
    void Save(SpanAdjustment item);
    bool Remove(string code);
    List<SpanAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanAdjustmentStore(Dictionary<string, SpanAdjustment> items, List<string> order) : ISpanAdjustmentStore
{
    public Dictionary<string, SpanAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanAdjustment item)
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

    public List<SpanAdjustment> List()
    {
        var outValue = new List<SpanAdjustment>();
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
public sealed class CachedSpanAdjustmentStore(MemorySpanAdjustmentStore inner, SpanAdjustmentCache cache, int hits, int misses) : ISpanAdjustmentStore
{
    public MemorySpanAdjustmentStore Inner { get; set; } = inner;
    public SpanAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanAdjustment? Find(string code)
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

    public void Save(SpanAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span adjustments. Empty fields match all.</summary>
public sealed record SpanAdjustmentQuery(string Locale, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(SpanAdjustment item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<SpanAdjustment> Apply(List<SpanAdjustment> items)
    {
        var matched = Collections.Filter(items, (SpanAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span adjustment store.</summary>
public static class SpanAdjustmentStore
{
    public static ISpanAdjustmentStore NewMemorySpanAdjustmentStore()
    {
        return new MemorySpanAdjustmentStore(new Dictionary<string, SpanAdjustment>(), new List<string>());
    }

    public static ISpanAdjustmentStore NewCachedSpanAdjustmentStore(MemorySpanAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedSpanAdjustmentStore(inner, SpanAdjustmentCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<SpanAdjustment> FindSpanAdjustments(ISpanAdjustmentStore store, SpanAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
