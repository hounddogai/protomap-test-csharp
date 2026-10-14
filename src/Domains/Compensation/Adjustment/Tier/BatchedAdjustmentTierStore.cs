namespace Acme.Workforce.Compensation.Adjustment.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched adjustment tiers by code.</summary>
public interface IBatchedAdjustmentTierStore
{
    BatchedAdjustmentTier? Find(string code);
    void Save(BatchedAdjustmentTier item);
    bool Remove(string code);
    List<BatchedAdjustmentTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedAdjustmentTierStore(Dictionary<string, BatchedAdjustmentTier> items, List<string> order) : IBatchedAdjustmentTierStore
{
    public Dictionary<string, BatchedAdjustmentTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedAdjustmentTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedAdjustmentTier item)
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

    public List<BatchedAdjustmentTier> List()
    {
        var outValue = new List<BatchedAdjustmentTier>();
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
public sealed class CachedBatchedAdjustmentTierStore(MemoryBatchedAdjustmentTierStore inner, BatchedAdjustmentTierCache cache, int hits, int misses) : IBatchedAdjustmentTierStore
{
    public MemoryBatchedAdjustmentTierStore Inner { get; set; } = inner;
    public BatchedAdjustmentTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedAdjustmentTier? Find(string code)
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

    public void Save(BatchedAdjustmentTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedAdjustmentTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched adjustment tiers. Empty fields match all.</summary>
public sealed record BatchedAdjustmentTierQuery(string Team, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedAdjustmentTier item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<BatchedAdjustmentTier> Apply(List<BatchedAdjustmentTier> items)
    {
        var matched = Collections.Filter(items, (BatchedAdjustmentTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched adjustment tier store.</summary>
public static class BatchedAdjustmentTierStore
{
    public static IBatchedAdjustmentTierStore NewMemoryBatchedAdjustmentTierStore()
    {
        return new MemoryBatchedAdjustmentTierStore(
            new Dictionary<string, BatchedAdjustmentTier>(),
            new List<string>());
    }

    public static IBatchedAdjustmentTierStore NewCachedBatchedAdjustmentTierStore(MemoryBatchedAdjustmentTierStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedAdjustmentTierStore(
            inner,
            BatchedAdjustmentTierCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<BatchedAdjustmentTier> FindBatchedAdjustmentTiers(IBatchedAdjustmentTierStore store, BatchedAdjustmentTierQuery query)
    {
        return query.Apply(store.List());
    }
}
