namespace Acme.Workforce.Talent.CareerPath.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched career path plans by code.</summary>
public interface IBatchedCareerPathPlanStore
{
    BatchedCareerPathPlan? Find(string code);
    void Save(BatchedCareerPathPlan item);
    bool Remove(string code);
    List<BatchedCareerPathPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedCareerPathPlanStore(Dictionary<string, BatchedCareerPathPlan> items, List<string> order) : IBatchedCareerPathPlanStore
{
    public Dictionary<string, BatchedCareerPathPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedCareerPathPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedCareerPathPlan item)
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

    public List<BatchedCareerPathPlan> List()
    {
        var outValue = new List<BatchedCareerPathPlan>();
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
public sealed class CachedBatchedCareerPathPlanStore(MemoryBatchedCareerPathPlanStore inner, BatchedCareerPathPlanCache cache, int hits, int misses) : IBatchedCareerPathPlanStore
{
    public MemoryBatchedCareerPathPlanStore Inner { get; set; } = inner;
    public BatchedCareerPathPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedCareerPathPlan? Find(string code)
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

    public void Save(BatchedCareerPathPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedCareerPathPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched career path plans. Empty fields match all.</summary>
public sealed record BatchedCareerPathPlanQuery(string Name, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedCareerPathPlan item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedCareerPathPlan> Apply(List<BatchedCareerPathPlan> items)
    {
        var matched = Collections.Filter(items, (BatchedCareerPathPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched career path plan store.</summary>
public static class BatchedCareerPathPlanStore
{
    public static IBatchedCareerPathPlanStore NewMemoryBatchedCareerPathPlanStore()
    {
        return new MemoryBatchedCareerPathPlanStore(
            new Dictionary<string, BatchedCareerPathPlan>(),
            new List<string>());
    }

    public static IBatchedCareerPathPlanStore NewCachedBatchedCareerPathPlanStore(MemoryBatchedCareerPathPlanStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedCareerPathPlanStore(
            inner,
            BatchedCareerPathPlanCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<BatchedCareerPathPlan> FindBatchedCareerPathPlans(IBatchedCareerPathPlanStore store, BatchedCareerPathPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
