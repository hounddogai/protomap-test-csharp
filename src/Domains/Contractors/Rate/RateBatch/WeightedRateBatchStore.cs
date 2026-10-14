namespace Acme.Workforce.Contractors.Rate.RateBatch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted rate batches by code.</summary>
public interface IWeightedRateBatchStore
{
    WeightedRateBatch? Find(string code);
    void Save(WeightedRateBatch item);
    bool Remove(string code);
    List<WeightedRateBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedRateBatchStore(Dictionary<string, WeightedRateBatch> items, List<string> order) : IWeightedRateBatchStore
{
    public Dictionary<string, WeightedRateBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedRateBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedRateBatch item)
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

    public List<WeightedRateBatch> List()
    {
        var outValue = new List<WeightedRateBatch>();
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
public sealed class CachedWeightedRateBatchStore(MemoryWeightedRateBatchStore inner, WeightedRateBatchCache cache, int hits, int misses) : IWeightedRateBatchStore
{
    public MemoryWeightedRateBatchStore Inner { get; set; } = inner;
    public WeightedRateBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedRateBatch? Find(string code)
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

    public void Save(WeightedRateBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedRateBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted rate batches. Empty fields match all.</summary>
public sealed record WeightedRateBatchQuery(string Title, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedRateBatch item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedRateBatch> Apply(List<WeightedRateBatch> items)
    {
        var matched = Collections.Filter(items, (WeightedRateBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted rate batch store.</summary>
public static class WeightedRateBatchStore
{
    public static IWeightedRateBatchStore NewMemoryWeightedRateBatchStore()
    {
        return new MemoryWeightedRateBatchStore(new Dictionary<string, WeightedRateBatch>(), new List<string>());
    }

    public static IWeightedRateBatchStore NewCachedWeightedRateBatchStore(MemoryWeightedRateBatchStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedRateBatchStore(inner, WeightedRateBatchCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<WeightedRateBatch> FindWeightedRateBatches(IWeightedRateBatchStore store, WeightedRateBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
