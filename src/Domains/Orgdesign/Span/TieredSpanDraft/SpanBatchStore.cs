namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span batches by code.</summary>
public interface ISpanBatchStore
{
    SpanBatch? Find(string code);
    void Save(SpanBatch item);
    bool Remove(string code);
    List<SpanBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanBatchStore(Dictionary<string, SpanBatch> items, List<string> order) : ISpanBatchStore
{
    public Dictionary<string, SpanBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanBatch item)
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

    public List<SpanBatch> List()
    {
        var outValue = new List<SpanBatch>();
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
public sealed class CachedSpanBatchStore(MemorySpanBatchStore inner, SpanBatchCache cache, int hits, int misses) : ISpanBatchStore
{
    public MemorySpanBatchStore Inner { get; set; } = inner;
    public SpanBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanBatch? Find(string code)
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

    public void Save(SpanBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span batches. Empty fields match all.</summary>
public sealed record SpanBatchQuery(string Department, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(SpanBatch item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanBatch> Apply(List<SpanBatch> items)
    {
        var matched = Collections.Filter(items, (SpanBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span batch store.</summary>
public static class SpanBatchStore
{
    public static ISpanBatchStore NewMemorySpanBatchStore()
    {
        return new MemorySpanBatchStore(new Dictionary<string, SpanBatch>(), new List<string>());
    }

    public static ISpanBatchStore NewCachedSpanBatchStore(MemorySpanBatchStore inner, int ttl, IClock clock)
    {
        return new CachedSpanBatchStore(inner, SpanBatchCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<SpanBatch> FindSpanBatches(ISpanBatchStore store, SpanBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
