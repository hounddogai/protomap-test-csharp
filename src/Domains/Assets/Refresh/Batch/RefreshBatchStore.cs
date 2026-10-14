namespace Acme.Workforce.Assets.Refresh.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh batches by code.</summary>
public interface IRefreshBatchStore
{
    RefreshBatch? Find(string code);
    void Save(RefreshBatch item);
    bool Remove(string code);
    List<RefreshBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshBatchStore(Dictionary<string, RefreshBatch> items, List<string> order) : IRefreshBatchStore
{
    public Dictionary<string, RefreshBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshBatch item)
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

    public List<RefreshBatch> List()
    {
        var outValue = new List<RefreshBatch>();
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
public sealed class CachedRefreshBatchStore(MemoryRefreshBatchStore inner, RefreshBatchCache cache, int hits, int misses) : IRefreshBatchStore
{
    public MemoryRefreshBatchStore Inner { get; set; } = inner;
    public RefreshBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshBatch? Find(string code)
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

    public void Save(RefreshBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh batches. Empty fields match all.</summary>
public sealed record RefreshBatchQuery(string Channel, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshBatch item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RefreshBatch> Apply(List<RefreshBatch> items)
    {
        var matched = Collections.Filter(items, (RefreshBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh batch store.</summary>
public static class RefreshBatchStore
{
    public static IRefreshBatchStore NewMemoryRefreshBatchStore()
    {
        return new MemoryRefreshBatchStore(new Dictionary<string, RefreshBatch>(), new List<string>());
    }

    public static IRefreshBatchStore NewCachedRefreshBatchStore(MemoryRefreshBatchStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshBatchStore(inner, RefreshBatchCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RefreshBatch> FindRefreshBatches(IRefreshBatchStore store, RefreshBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
