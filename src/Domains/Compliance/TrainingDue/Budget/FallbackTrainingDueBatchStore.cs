namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback training due batches by code.</summary>
public interface IFallbackTrainingDueBatchStore
{
    FallbackTrainingDueBatch? Find(string code);
    void Save(FallbackTrainingDueBatch item);
    bool Remove(string code);
    List<FallbackTrainingDueBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackTrainingDueBatchStore(Dictionary<string, FallbackTrainingDueBatch> items, List<string> order) : IFallbackTrainingDueBatchStore
{
    public Dictionary<string, FallbackTrainingDueBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackTrainingDueBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackTrainingDueBatch item)
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

    public List<FallbackTrainingDueBatch> List()
    {
        var outValue = new List<FallbackTrainingDueBatch>();
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
public sealed class CachedFallbackTrainingDueBatchStore(MemoryFallbackTrainingDueBatchStore inner, FallbackTrainingDueBatchCache cache, int hits, int misses) : IFallbackTrainingDueBatchStore
{
    public MemoryFallbackTrainingDueBatchStore Inner { get; set; } = inner;
    public FallbackTrainingDueBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackTrainingDueBatch? Find(string code)
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

    public void Save(FallbackTrainingDueBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackTrainingDueBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback training due batches. Empty fields match all.</summary>
public sealed record FallbackTrainingDueBatchQuery(string Region, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackTrainingDueBatch item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackTrainingDueBatch> Apply(List<FallbackTrainingDueBatch> items)
    {
        var matched = Collections.Filter(items, (FallbackTrainingDueBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback training due batch store.</summary>
public static class FallbackTrainingDueBatchStore
{
    public static IFallbackTrainingDueBatchStore NewMemoryFallbackTrainingDueBatchStore()
    {
        return new MemoryFallbackTrainingDueBatchStore(
            new Dictionary<string, FallbackTrainingDueBatch>(),
            new List<string>());
    }

    public static IFallbackTrainingDueBatchStore NewCachedFallbackTrainingDueBatchStore(MemoryFallbackTrainingDueBatchStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackTrainingDueBatchStore(
            inner,
            FallbackTrainingDueBatchCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<FallbackTrainingDueBatch> FindFallbackTrainingDueBatches(IFallbackTrainingDueBatchStore store, FallbackTrainingDueBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
