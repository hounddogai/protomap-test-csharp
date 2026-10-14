namespace Acme.Workforce.Compensation.Adjustment.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched adjustment rules by code.</summary>
public interface IBatchedAdjustmentRuleStore
{
    BatchedAdjustmentRule? Find(string code);
    void Save(BatchedAdjustmentRule item);
    bool Remove(string code);
    List<BatchedAdjustmentRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedAdjustmentRuleStore(Dictionary<string, BatchedAdjustmentRule> items, List<string> order) : IBatchedAdjustmentRuleStore
{
    public Dictionary<string, BatchedAdjustmentRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedAdjustmentRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedAdjustmentRule item)
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

    public List<BatchedAdjustmentRule> List()
    {
        var outValue = new List<BatchedAdjustmentRule>();
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
public sealed class CachedBatchedAdjustmentRuleStore(MemoryBatchedAdjustmentRuleStore inner, BatchedAdjustmentRuleCache cache, int hits, int misses) : IBatchedAdjustmentRuleStore
{
    public MemoryBatchedAdjustmentRuleStore Inner { get; set; } = inner;
    public BatchedAdjustmentRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedAdjustmentRule? Find(string code)
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

    public void Save(BatchedAdjustmentRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedAdjustmentRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched adjustment rules. Empty fields match all.</summary>
public sealed record BatchedAdjustmentRuleQuery(string Team, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedAdjustmentRule item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedAdjustmentRule> Apply(List<BatchedAdjustmentRule> items)
    {
        var matched = Collections.Filter(items, (BatchedAdjustmentRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched adjustment rule store.</summary>
public static class BatchedAdjustmentRuleStore
{
    public static IBatchedAdjustmentRuleStore NewMemoryBatchedAdjustmentRuleStore()
    {
        return new MemoryBatchedAdjustmentRuleStore(
            new Dictionary<string, BatchedAdjustmentRule>(),
            new List<string>());
    }

    public static IBatchedAdjustmentRuleStore NewCachedBatchedAdjustmentRuleStore(MemoryBatchedAdjustmentRuleStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedAdjustmentRuleStore(inner, BatchedAdjustmentRuleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BatchedAdjustmentRule> FindBatchedAdjustmentRules(IBatchedAdjustmentRuleStore store, BatchedAdjustmentRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
