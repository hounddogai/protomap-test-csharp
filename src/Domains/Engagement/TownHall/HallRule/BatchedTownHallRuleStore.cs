namespace Acme.Workforce.Engagement.TownHall.HallRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched town hall rules by code.</summary>
public interface IBatchedTownHallRuleStore
{
    BatchedTownHallRule? Find(string code);
    void Save(BatchedTownHallRule item);
    bool Remove(string code);
    List<BatchedTownHallRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedTownHallRuleStore(Dictionary<string, BatchedTownHallRule> items, List<string> order) : IBatchedTownHallRuleStore
{
    public Dictionary<string, BatchedTownHallRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedTownHallRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedTownHallRule item)
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

    public List<BatchedTownHallRule> List()
    {
        var outValue = new List<BatchedTownHallRule>();
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
public sealed class CachedBatchedTownHallRuleStore(MemoryBatchedTownHallRuleStore inner, BatchedTownHallRuleCache cache, int hits, int misses) : IBatchedTownHallRuleStore
{
    public MemoryBatchedTownHallRuleStore Inner { get; set; } = inner;
    public BatchedTownHallRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedTownHallRule? Find(string code)
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

    public void Save(BatchedTownHallRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedTownHallRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched town hall rules. Empty fields match all.</summary>
public sealed record BatchedTownHallRuleQuery(string Unit, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedTownHallRule item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedTownHallRule> Apply(List<BatchedTownHallRule> items)
    {
        var matched = Collections.Filter(items, (BatchedTownHallRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched town hall rule store.</summary>
public static class BatchedTownHallRuleStore
{
    public static IBatchedTownHallRuleStore NewMemoryBatchedTownHallRuleStore()
    {
        return new MemoryBatchedTownHallRuleStore(new Dictionary<string, BatchedTownHallRule>(), new List<string>());
    }

    public static IBatchedTownHallRuleStore NewCachedBatchedTownHallRuleStore(MemoryBatchedTownHallRuleStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedTownHallRuleStore(inner, BatchedTownHallRuleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BatchedTownHallRule> FindBatchedTownHallRules(IBatchedTownHallRuleStore store, BatchedTownHallRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
