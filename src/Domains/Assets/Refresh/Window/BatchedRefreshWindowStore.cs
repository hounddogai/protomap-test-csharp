namespace Acme.Workforce.Assets.Refresh.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched refresh windows by code.</summary>
public interface IBatchedRefreshWindowStore
{
    BatchedRefreshWindow? Find(string code);
    void Save(BatchedRefreshWindow item);
    bool Remove(string code);
    List<BatchedRefreshWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedRefreshWindowStore(Dictionary<string, BatchedRefreshWindow> items, List<string> order) : IBatchedRefreshWindowStore
{
    public Dictionary<string, BatchedRefreshWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedRefreshWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedRefreshWindow item)
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

    public List<BatchedRefreshWindow> List()
    {
        var outValue = new List<BatchedRefreshWindow>();
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
public sealed class CachedBatchedRefreshWindowStore(MemoryBatchedRefreshWindowStore inner, BatchedRefreshWindowCache cache, int hits, int misses) : IBatchedRefreshWindowStore
{
    public MemoryBatchedRefreshWindowStore Inner { get; set; } = inner;
    public BatchedRefreshWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedRefreshWindow? Find(string code)
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

    public void Save(BatchedRefreshWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedRefreshWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched refresh windows. Empty fields match all.</summary>
public sealed record BatchedRefreshWindowQuery(string Label, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedRefreshWindow item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedRefreshWindow> Apply(List<BatchedRefreshWindow> items)
    {
        var matched = Collections.Filter(items, (BatchedRefreshWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched refresh window store.</summary>
public static class BatchedRefreshWindowStore
{
    public static IBatchedRefreshWindowStore NewMemoryBatchedRefreshWindowStore()
    {
        return new MemoryBatchedRefreshWindowStore(
            new Dictionary<string, BatchedRefreshWindow>(),
            new List<string>());
    }

    public static IBatchedRefreshWindowStore NewCachedBatchedRefreshWindowStore(MemoryBatchedRefreshWindowStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedRefreshWindowStore(inner, BatchedRefreshWindowCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BatchedRefreshWindow> FindBatchedRefreshWindows(IBatchedRefreshWindowStore store, BatchedRefreshWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
