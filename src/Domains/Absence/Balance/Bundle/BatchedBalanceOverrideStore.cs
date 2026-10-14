namespace Acme.Workforce.Absence.Balance.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched balance overrides by code.</summary>
public interface IBatchedBalanceOverrideStore
{
    BatchedBalanceOverride? Find(string code);
    void Save(BatchedBalanceOverride item);
    bool Remove(string code);
    List<BatchedBalanceOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedBalanceOverrideStore(Dictionary<string, BatchedBalanceOverride> items, List<string> order) : IBatchedBalanceOverrideStore
{
    public Dictionary<string, BatchedBalanceOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedBalanceOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedBalanceOverride item)
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

    public List<BatchedBalanceOverride> List()
    {
        var outValue = new List<BatchedBalanceOverride>();
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
public sealed class CachedBatchedBalanceOverrideStore(MemoryBatchedBalanceOverrideStore inner, BatchedBalanceOverrideCache cache, int hits, int misses) : IBatchedBalanceOverrideStore
{
    public MemoryBatchedBalanceOverrideStore Inner { get; set; } = inner;
    public BatchedBalanceOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedBalanceOverride? Find(string code)
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

    public void Save(BatchedBalanceOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedBalanceOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched balance overrides. Empty fields match all.</summary>
public sealed record BatchedBalanceOverrideQuery(string Name, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedBalanceOverride item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedBalanceOverride> Apply(List<BatchedBalanceOverride> items)
    {
        var matched = Collections.Filter(items, (BatchedBalanceOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched balance override store.</summary>
public static class BatchedBalanceOverrideStore
{
    public static IBatchedBalanceOverrideStore NewMemoryBatchedBalanceOverrideStore()
    {
        return new MemoryBatchedBalanceOverrideStore(
            new Dictionary<string, BatchedBalanceOverride>(),
            new List<string>());
    }

    public static IBatchedBalanceOverrideStore NewCachedBatchedBalanceOverrideStore(MemoryBatchedBalanceOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedBalanceOverrideStore(
            inner,
            BatchedBalanceOverrideCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<BatchedBalanceOverride> FindBatchedBalanceOverrides(IBatchedBalanceOverrideStore store, BatchedBalanceOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
