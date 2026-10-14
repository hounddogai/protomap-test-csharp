namespace Acme.Workforce.Contractors.Timesheet.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched timesheet estimates by code.</summary>
public interface IBatchedTimesheetEstimateStore
{
    BatchedTimesheetEstimate? Find(string code);
    void Save(BatchedTimesheetEstimate item);
    bool Remove(string code);
    List<BatchedTimesheetEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedTimesheetEstimateStore(Dictionary<string, BatchedTimesheetEstimate> items, List<string> order) : IBatchedTimesheetEstimateStore
{
    public Dictionary<string, BatchedTimesheetEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedTimesheetEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedTimesheetEstimate item)
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

    public List<BatchedTimesheetEstimate> List()
    {
        var outValue = new List<BatchedTimesheetEstimate>();
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
public sealed class CachedBatchedTimesheetEstimateStore(MemoryBatchedTimesheetEstimateStore inner, BatchedTimesheetEstimateCache cache, int hits, int misses) : IBatchedTimesheetEstimateStore
{
    public MemoryBatchedTimesheetEstimateStore Inner { get; set; } = inner;
    public BatchedTimesheetEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedTimesheetEstimate? Find(string code)
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

    public void Save(BatchedTimesheetEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedTimesheetEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched timesheet estimates. Empty fields match all.</summary>
public sealed record BatchedTimesheetEstimateQuery(string Title, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedTimesheetEstimate item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedTimesheetEstimate> Apply(List<BatchedTimesheetEstimate> items)
    {
        var matched = Collections.Filter(items, (BatchedTimesheetEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched timesheet estimate store.</summary>
public static class BatchedTimesheetEstimateStore
{
    public static IBatchedTimesheetEstimateStore NewMemoryBatchedTimesheetEstimateStore()
    {
        return new MemoryBatchedTimesheetEstimateStore(
            new Dictionary<string, BatchedTimesheetEstimate>(),
            new List<string>());
    }

    public static IBatchedTimesheetEstimateStore NewCachedBatchedTimesheetEstimateStore(MemoryBatchedTimesheetEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedTimesheetEstimateStore(
            inner,
            BatchedTimesheetEstimateCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<BatchedTimesheetEstimate> FindBatchedTimesheetEstimates(IBatchedTimesheetEstimateStore store, BatchedTimesheetEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
