namespace Acme.Workforce.Payrollops.RetroPay.PayPolicy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched retro pay plans by code.</summary>
public interface IBatchedRetroPayPlanStore
{
    BatchedRetroPayPlan? Find(string code);
    void Save(BatchedRetroPayPlan item);
    bool Remove(string code);
    List<BatchedRetroPayPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedRetroPayPlanStore(Dictionary<string, BatchedRetroPayPlan> items, List<string> order) : IBatchedRetroPayPlanStore
{
    public Dictionary<string, BatchedRetroPayPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedRetroPayPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedRetroPayPlan item)
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

    public List<BatchedRetroPayPlan> List()
    {
        var outValue = new List<BatchedRetroPayPlan>();
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
public sealed class CachedBatchedRetroPayPlanStore(MemoryBatchedRetroPayPlanStore inner, BatchedRetroPayPlanCache cache, int hits, int misses) : IBatchedRetroPayPlanStore
{
    public MemoryBatchedRetroPayPlanStore Inner { get; set; } = inner;
    public BatchedRetroPayPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedRetroPayPlan? Find(string code)
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

    public void Save(BatchedRetroPayPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedRetroPayPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched retro pay plans. Empty fields match all.</summary>
public sealed record BatchedRetroPayPlanQuery(string Name, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedRetroPayPlan item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedRetroPayPlan> Apply(List<BatchedRetroPayPlan> items)
    {
        var matched = Collections.Filter(items, (BatchedRetroPayPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched retro pay plan store.</summary>
public static class BatchedRetroPayPlanStore
{
    public static IBatchedRetroPayPlanStore NewMemoryBatchedRetroPayPlanStore()
    {
        return new MemoryBatchedRetroPayPlanStore(new Dictionary<string, BatchedRetroPayPlan>(), new List<string>());
    }

    public static IBatchedRetroPayPlanStore NewCachedBatchedRetroPayPlanStore(MemoryBatchedRetroPayPlanStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedRetroPayPlanStore(inner, BatchedRetroPayPlanCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BatchedRetroPayPlan> FindBatchedRetroPayPlans(IBatchedRetroPayPlanStore store, BatchedRetroPayPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
