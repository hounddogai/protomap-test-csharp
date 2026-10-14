namespace Acme.Workforce.Compliance.Exception.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception batches by code.</summary>
public interface IExceptionBatchStore
{
    ExceptionBatch? Find(string code);
    void Save(ExceptionBatch item);
    bool Remove(string code);
    List<ExceptionBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionBatchStore(Dictionary<string, ExceptionBatch> items, List<string> order) : IExceptionBatchStore
{
    public Dictionary<string, ExceptionBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionBatch item)
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

    public List<ExceptionBatch> List()
    {
        var outValue = new List<ExceptionBatch>();
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
public sealed class CachedExceptionBatchStore(MemoryExceptionBatchStore inner, ExceptionBatchCache cache, int hits, int misses) : IExceptionBatchStore
{
    public MemoryExceptionBatchStore Inner { get; set; } = inner;
    public ExceptionBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionBatch? Find(string code)
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

    public void Save(ExceptionBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception batches. Empty fields match all.</summary>
public sealed record ExceptionBatchQuery(string Team, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionBatch item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ExceptionBatch> Apply(List<ExceptionBatch> items)
    {
        var matched = Collections.Filter(items, (ExceptionBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception batch store.</summary>
public static class ExceptionBatchStore
{
    public static IExceptionBatchStore NewMemoryExceptionBatchStore()
    {
        return new MemoryExceptionBatchStore(new Dictionary<string, ExceptionBatch>(), new List<string>());
    }

    public static IExceptionBatchStore NewCachedExceptionBatchStore(MemoryExceptionBatchStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionBatchStore(inner, ExceptionBatchCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ExceptionBatch> FindExceptionBatches(IExceptionBatchStore store, ExceptionBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
