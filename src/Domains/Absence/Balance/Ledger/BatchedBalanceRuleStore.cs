namespace Acme.Workforce.Absence.Balance.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched balance rules by code.</summary>
public interface IBatchedBalanceRuleStore
{
    BatchedBalanceRule? Find(string code);
    void Save(BatchedBalanceRule item);
    bool Remove(string code);
    List<BatchedBalanceRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedBalanceRuleStore(Dictionary<string, BatchedBalanceRule> items, List<string> order) : IBatchedBalanceRuleStore
{
    public Dictionary<string, BatchedBalanceRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedBalanceRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedBalanceRule item)
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

    public List<BatchedBalanceRule> List()
    {
        var outValue = new List<BatchedBalanceRule>();
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
public sealed class CachedBatchedBalanceRuleStore(MemoryBatchedBalanceRuleStore inner, BatchedBalanceRuleCache cache, int hits, int misses) : IBatchedBalanceRuleStore
{
    public MemoryBatchedBalanceRuleStore Inner { get; set; } = inner;
    public BatchedBalanceRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedBalanceRule? Find(string code)
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

    public void Save(BatchedBalanceRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedBalanceRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched balance rules. Empty fields match all.</summary>
public sealed record BatchedBalanceRuleQuery(string Name, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedBalanceRule item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedBalanceRule> Apply(List<BatchedBalanceRule> items)
    {
        var matched = Collections.Filter(items, (BatchedBalanceRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched balance rule store.</summary>
public static class BatchedBalanceRuleStore
{
    public static IBatchedBalanceRuleStore NewMemoryBatchedBalanceRuleStore()
    {
        return new MemoryBatchedBalanceRuleStore(new Dictionary<string, BatchedBalanceRule>(), new List<string>());
    }

    public static IBatchedBalanceRuleStore NewCachedBatchedBalanceRuleStore(MemoryBatchedBalanceRuleStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedBalanceRuleStore(inner, BatchedBalanceRuleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BatchedBalanceRule> FindBatchedBalanceRules(IBatchedBalanceRuleStore store, BatchedBalanceRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
