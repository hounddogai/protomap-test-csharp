namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched pay band thresholds by code.</summary>
public interface IBatchedPayBandThresholdStore
{
    BatchedPayBandThreshold? Find(string code);
    void Save(BatchedPayBandThreshold item);
    bool Remove(string code);
    List<BatchedPayBandThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedPayBandThresholdStore(Dictionary<string, BatchedPayBandThreshold> items, List<string> order) : IBatchedPayBandThresholdStore
{
    public Dictionary<string, BatchedPayBandThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedPayBandThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedPayBandThreshold item)
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

    public List<BatchedPayBandThreshold> List()
    {
        var outValue = new List<BatchedPayBandThreshold>();
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
public sealed class CachedBatchedPayBandThresholdStore(MemoryBatchedPayBandThresholdStore inner, BatchedPayBandThresholdCache cache, int hits, int misses) : IBatchedPayBandThresholdStore
{
    public MemoryBatchedPayBandThresholdStore Inner { get; set; } = inner;
    public BatchedPayBandThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedPayBandThreshold? Find(string code)
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

    public void Save(BatchedPayBandThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedPayBandThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched pay band thresholds. Empty fields match all.</summary>
public sealed record BatchedPayBandThresholdQuery(string Country, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedPayBandThreshold item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedPayBandThreshold> Apply(List<BatchedPayBandThreshold> items)
    {
        var matched = Collections.Filter(items, (BatchedPayBandThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched pay band threshold store.</summary>
public static class BatchedPayBandThresholdStore
{
    public static IBatchedPayBandThresholdStore NewMemoryBatchedPayBandThresholdStore()
    {
        return new MemoryBatchedPayBandThresholdStore(
            new Dictionary<string, BatchedPayBandThreshold>(),
            new List<string>());
    }

    public static IBatchedPayBandThresholdStore NewCachedBatchedPayBandThresholdStore(MemoryBatchedPayBandThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedPayBandThresholdStore(
            inner,
            BatchedPayBandThresholdCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<BatchedPayBandThreshold> FindBatchedPayBandThresholds(IBatchedPayBandThresholdStore store, BatchedPayBandThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
