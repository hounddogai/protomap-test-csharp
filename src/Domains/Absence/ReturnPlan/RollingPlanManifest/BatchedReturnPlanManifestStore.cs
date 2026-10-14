namespace Acme.Workforce.Absence.ReturnPlan.RollingPlanManifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched return plan manifests by code.</summary>
public interface IBatchedReturnPlanManifestStore
{
    BatchedReturnPlanManifest? Find(string code);
    void Save(BatchedReturnPlanManifest item);
    bool Remove(string code);
    List<BatchedReturnPlanManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedReturnPlanManifestStore(Dictionary<string, BatchedReturnPlanManifest> items, List<string> order) : IBatchedReturnPlanManifestStore
{
    public Dictionary<string, BatchedReturnPlanManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedReturnPlanManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedReturnPlanManifest item)
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

    public List<BatchedReturnPlanManifest> List()
    {
        var outValue = new List<BatchedReturnPlanManifest>();
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
public sealed class CachedBatchedReturnPlanManifestStore(MemoryBatchedReturnPlanManifestStore inner, BatchedReturnPlanManifestCache cache, int hits, int misses) : IBatchedReturnPlanManifestStore
{
    public MemoryBatchedReturnPlanManifestStore Inner { get; set; } = inner;
    public BatchedReturnPlanManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedReturnPlanManifest? Find(string code)
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

    public void Save(BatchedReturnPlanManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedReturnPlanManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched return plan manifests. Empty fields match all.</summary>
public sealed record BatchedReturnPlanManifestQuery(string Segment, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedReturnPlanManifest item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<BatchedReturnPlanManifest> Apply(List<BatchedReturnPlanManifest> items)
    {
        var matched = Collections.Filter(items, (BatchedReturnPlanManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched return plan manifest store.</summary>
public static class BatchedReturnPlanManifestStore
{
    public static IBatchedReturnPlanManifestStore NewMemoryBatchedReturnPlanManifestStore()
    {
        return new MemoryBatchedReturnPlanManifestStore(
            new Dictionary<string, BatchedReturnPlanManifest>(),
            new List<string>());
    }

    public static IBatchedReturnPlanManifestStore NewCachedBatchedReturnPlanManifestStore(MemoryBatchedReturnPlanManifestStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedReturnPlanManifestStore(
            inner,
            BatchedReturnPlanManifestCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<BatchedReturnPlanManifest> FindBatchedReturnPlanManifests(IBatchedReturnPlanManifestStore store, BatchedReturnPlanManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
