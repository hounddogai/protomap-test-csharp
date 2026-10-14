namespace Acme.Workforce.Compensation.BonusPool.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched bonus pool thresholds by code.</summary>
public interface IBatchedBonusPoolThresholdStore
{
    BatchedBonusPoolThreshold? Find(string code);
    void Save(BatchedBonusPoolThreshold item);
    bool Remove(string code);
    List<BatchedBonusPoolThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedBonusPoolThresholdStore(Dictionary<string, BatchedBonusPoolThreshold> items, List<string> order) : IBatchedBonusPoolThresholdStore
{
    public Dictionary<string, BatchedBonusPoolThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedBonusPoolThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedBonusPoolThreshold item)
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

    public List<BatchedBonusPoolThreshold> List()
    {
        var outValue = new List<BatchedBonusPoolThreshold>();
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
public sealed class CachedBatchedBonusPoolThresholdStore(MemoryBatchedBonusPoolThresholdStore inner, BatchedBonusPoolThresholdCache cache, int hits, int misses) : IBatchedBonusPoolThresholdStore
{
    public MemoryBatchedBonusPoolThresholdStore Inner { get; set; } = inner;
    public BatchedBonusPoolThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedBonusPoolThreshold? Find(string code)
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

    public void Save(BatchedBonusPoolThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedBonusPoolThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched bonus pool thresholds. Empty fields match all.</summary>
public sealed record BatchedBonusPoolThresholdQuery(string Unit, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedBonusPoolThreshold item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedBonusPoolThreshold> Apply(List<BatchedBonusPoolThreshold> items)
    {
        var matched = Collections.Filter(items, (BatchedBonusPoolThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched bonus pool threshold store.</summary>
public static class BatchedBonusPoolThresholdStore
{
    public static IBatchedBonusPoolThresholdStore NewMemoryBatchedBonusPoolThresholdStore()
    {
        return new MemoryBatchedBonusPoolThresholdStore(
            new Dictionary<string, BatchedBonusPoolThreshold>(),
            new List<string>());
    }

    public static IBatchedBonusPoolThresholdStore NewCachedBatchedBonusPoolThresholdStore(MemoryBatchedBonusPoolThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedBonusPoolThresholdStore(
            inner,
            BatchedBonusPoolThresholdCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<BatchedBonusPoolThreshold> FindBatchedBonusPoolThresholds(IBatchedBonusPoolThresholdStore store, BatchedBonusPoolThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
