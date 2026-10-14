namespace Acme.Workforce.Payrollops.Garnishment.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched garnishment exceptions by code.</summary>
public interface IBatchedGarnishmentExceptionStore
{
    BatchedGarnishmentException? Find(string code);
    void Save(BatchedGarnishmentException item);
    bool Remove(string code);
    List<BatchedGarnishmentException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedGarnishmentExceptionStore(Dictionary<string, BatchedGarnishmentException> items, List<string> order) : IBatchedGarnishmentExceptionStore
{
    public Dictionary<string, BatchedGarnishmentException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedGarnishmentException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedGarnishmentException item)
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

    public List<BatchedGarnishmentException> List()
    {
        var outValue = new List<BatchedGarnishmentException>();
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
public sealed class CachedBatchedGarnishmentExceptionStore(MemoryBatchedGarnishmentExceptionStore inner, BatchedGarnishmentExceptionCache cache, int hits, int misses) : IBatchedGarnishmentExceptionStore
{
    public MemoryBatchedGarnishmentExceptionStore Inner { get; set; } = inner;
    public BatchedGarnishmentExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedGarnishmentException? Find(string code)
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

    public void Save(BatchedGarnishmentException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedGarnishmentException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched garnishment exceptions. Empty fields match all.</summary>
public sealed record BatchedGarnishmentExceptionQuery(string Title, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedGarnishmentException item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<BatchedGarnishmentException> Apply(List<BatchedGarnishmentException> items)
    {
        var matched = Collections.Filter(items, (BatchedGarnishmentException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched garnishment exception store.</summary>
public static class BatchedGarnishmentExceptionStore
{
    public static IBatchedGarnishmentExceptionStore NewMemoryBatchedGarnishmentExceptionStore()
    {
        return new MemoryBatchedGarnishmentExceptionStore(
            new Dictionary<string, BatchedGarnishmentException>(),
            new List<string>());
    }

    public static IBatchedGarnishmentExceptionStore NewCachedBatchedGarnishmentExceptionStore(MemoryBatchedGarnishmentExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedGarnishmentExceptionStore(
            inner,
            BatchedGarnishmentExceptionCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<BatchedGarnishmentException> FindBatchedGarnishmentExceptions(IBatchedGarnishmentExceptionStore store, BatchedGarnishmentExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
