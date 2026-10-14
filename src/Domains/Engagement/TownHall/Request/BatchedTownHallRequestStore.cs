namespace Acme.Workforce.Engagement.TownHall.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched town hall requests by code.</summary>
public interface IBatchedTownHallRequestStore
{
    BatchedTownHallRequest? Find(string code);
    void Save(BatchedTownHallRequest item);
    bool Remove(string code);
    List<BatchedTownHallRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedTownHallRequestStore(Dictionary<string, BatchedTownHallRequest> items, List<string> order) : IBatchedTownHallRequestStore
{
    public Dictionary<string, BatchedTownHallRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedTownHallRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedTownHallRequest item)
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

    public List<BatchedTownHallRequest> List()
    {
        var outValue = new List<BatchedTownHallRequest>();
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
public sealed class CachedBatchedTownHallRequestStore(MemoryBatchedTownHallRequestStore inner, BatchedTownHallRequestCache cache, int hits, int misses) : IBatchedTownHallRequestStore
{
    public MemoryBatchedTownHallRequestStore Inner { get; set; } = inner;
    public BatchedTownHallRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedTownHallRequest? Find(string code)
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

    public void Save(BatchedTownHallRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedTownHallRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched town hall requests. Empty fields match all.</summary>
public sealed record BatchedTownHallRequestQuery(string Title, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedTownHallRequest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<BatchedTownHallRequest> Apply(List<BatchedTownHallRequest> items)
    {
        var matched = Collections.Filter(items, (BatchedTownHallRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched town hall request store.</summary>
public static class BatchedTownHallRequestStore
{
    public static IBatchedTownHallRequestStore NewMemoryBatchedTownHallRequestStore()
    {
        return new MemoryBatchedTownHallRequestStore(
            new Dictionary<string, BatchedTownHallRequest>(),
            new List<string>());
    }

    public static IBatchedTownHallRequestStore NewCachedBatchedTownHallRequestStore(MemoryBatchedTownHallRequestStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedTownHallRequestStore(
            inner,
            BatchedTownHallRequestCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<BatchedTownHallRequest> FindBatchedTownHallRequests(IBatchedTownHallRequestStore store, BatchedTownHallRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
