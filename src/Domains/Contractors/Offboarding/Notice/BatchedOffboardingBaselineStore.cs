namespace Acme.Workforce.Contractors.Offboarding.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched offboarding baselines by code.</summary>
public interface IBatchedOffboardingBaselineStore
{
    BatchedOffboardingBaseline? Find(string code);
    void Save(BatchedOffboardingBaseline item);
    bool Remove(string code);
    List<BatchedOffboardingBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedOffboardingBaselineStore(Dictionary<string, BatchedOffboardingBaseline> items, List<string> order) : IBatchedOffboardingBaselineStore
{
    public Dictionary<string, BatchedOffboardingBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedOffboardingBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedOffboardingBaseline item)
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

    public List<BatchedOffboardingBaseline> List()
    {
        var outValue = new List<BatchedOffboardingBaseline>();
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
public sealed class CachedBatchedOffboardingBaselineStore(MemoryBatchedOffboardingBaselineStore inner, BatchedOffboardingBaselineCache cache, int hits, int misses) : IBatchedOffboardingBaselineStore
{
    public MemoryBatchedOffboardingBaselineStore Inner { get; set; } = inner;
    public BatchedOffboardingBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedOffboardingBaseline? Find(string code)
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

    public void Save(BatchedOffboardingBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedOffboardingBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched offboarding baselines. Empty fields match all.</summary>
public sealed record BatchedOffboardingBaselineQuery(string Department, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedOffboardingBaseline item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedOffboardingBaseline> Apply(List<BatchedOffboardingBaseline> items)
    {
        var matched = Collections.Filter(items, (BatchedOffboardingBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched offboarding baseline store.</summary>
public static class BatchedOffboardingBaselineStore
{
    public static IBatchedOffboardingBaselineStore NewMemoryBatchedOffboardingBaselineStore()
    {
        return new MemoryBatchedOffboardingBaselineStore(
            new Dictionary<string, BatchedOffboardingBaseline>(),
            new List<string>());
    }

    public static IBatchedOffboardingBaselineStore NewCachedBatchedOffboardingBaselineStore(MemoryBatchedOffboardingBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedOffboardingBaselineStore(
            inner,
            BatchedOffboardingBaselineCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<BatchedOffboardingBaseline> FindBatchedOffboardingBaselines(IBatchedOffboardingBaselineStore store, BatchedOffboardingBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
