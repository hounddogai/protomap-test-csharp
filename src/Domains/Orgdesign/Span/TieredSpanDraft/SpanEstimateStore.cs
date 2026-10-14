namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span estimates by code.</summary>
public interface ISpanEstimateStore
{
    SpanEstimate? Find(string code);
    void Save(SpanEstimate item);
    bool Remove(string code);
    List<SpanEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanEstimateStore(Dictionary<string, SpanEstimate> items, List<string> order) : ISpanEstimateStore
{
    public Dictionary<string, SpanEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanEstimate item)
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

    public List<SpanEstimate> List()
    {
        var outValue = new List<SpanEstimate>();
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
public sealed class CachedSpanEstimateStore(MemorySpanEstimateStore inner, SpanEstimateCache cache, int hits, int misses) : ISpanEstimateStore
{
    public MemorySpanEstimateStore Inner { get; set; } = inner;
    public SpanEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanEstimate? Find(string code)
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

    public void Save(SpanEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span estimates. Empty fields match all.</summary>
public sealed record SpanEstimateQuery(string Department, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(SpanEstimate item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<SpanEstimate> Apply(List<SpanEstimate> items)
    {
        var matched = Collections.Filter(items, (SpanEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span estimate store.</summary>
public static class SpanEstimateStore
{
    public static ISpanEstimateStore NewMemorySpanEstimateStore()
    {
        return new MemorySpanEstimateStore(new Dictionary<string, SpanEstimate>(), new List<string>());
    }

    public static ISpanEstimateStore NewCachedSpanEstimateStore(MemorySpanEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedSpanEstimateStore(inner, SpanEstimateCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SpanEstimate> FindSpanEstimates(ISpanEstimateStore store, SpanEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
