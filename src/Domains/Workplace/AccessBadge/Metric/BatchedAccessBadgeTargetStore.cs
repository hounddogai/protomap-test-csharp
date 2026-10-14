namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched access badge targets by code.</summary>
public interface IBatchedAccessBadgeTargetStore
{
    BatchedAccessBadgeTarget? Find(string code);
    void Save(BatchedAccessBadgeTarget item);
    bool Remove(string code);
    List<BatchedAccessBadgeTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedAccessBadgeTargetStore(Dictionary<string, BatchedAccessBadgeTarget> items, List<string> order) : IBatchedAccessBadgeTargetStore
{
    public Dictionary<string, BatchedAccessBadgeTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedAccessBadgeTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedAccessBadgeTarget item)
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

    public List<BatchedAccessBadgeTarget> List()
    {
        var outValue = new List<BatchedAccessBadgeTarget>();
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
public sealed class CachedBatchedAccessBadgeTargetStore(MemoryBatchedAccessBadgeTargetStore inner, BatchedAccessBadgeTargetCache cache, int hits, int misses) : IBatchedAccessBadgeTargetStore
{
    public MemoryBatchedAccessBadgeTargetStore Inner { get; set; } = inner;
    public BatchedAccessBadgeTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedAccessBadgeTarget? Find(string code)
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

    public void Save(BatchedAccessBadgeTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedAccessBadgeTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched access badge targets. Empty fields match all.</summary>
public sealed record BatchedAccessBadgeTargetQuery(string Country, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedAccessBadgeTarget item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedAccessBadgeTarget> Apply(List<BatchedAccessBadgeTarget> items)
    {
        var matched = Collections.Filter(items, (BatchedAccessBadgeTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched access badge target store.</summary>
public static class BatchedAccessBadgeTargetStore
{
    public static IBatchedAccessBadgeTargetStore NewMemoryBatchedAccessBadgeTargetStore()
    {
        return new MemoryBatchedAccessBadgeTargetStore(
            new Dictionary<string, BatchedAccessBadgeTarget>(),
            new List<string>());
    }

    public static IBatchedAccessBadgeTargetStore NewCachedBatchedAccessBadgeTargetStore(MemoryBatchedAccessBadgeTargetStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedAccessBadgeTargetStore(
            inner,
            BatchedAccessBadgeTargetCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<BatchedAccessBadgeTarget> FindBatchedAccessBadgeTargets(IBatchedAccessBadgeTargetStore store, BatchedAccessBadgeTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
