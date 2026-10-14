namespace Acme.Workforce.Contractors.Rate.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate batches by code.</summary>
public interface IRateBatchStore
{
    RateBatch? Find(string code);
    void Save(RateBatch item);
    bool Remove(string code);
    List<RateBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateBatchStore(Dictionary<string, RateBatch> items, List<string> order) : IRateBatchStore
{
    public Dictionary<string, RateBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateBatch item)
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

    public List<RateBatch> List()
    {
        var outValue = new List<RateBatch>();
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
public sealed class CachedRateBatchStore(MemoryRateBatchStore inner, RateBatchCache cache, int hits, int misses) : IRateBatchStore
{
    public MemoryRateBatchStore Inner { get; set; } = inner;
    public RateBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateBatch? Find(string code)
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

    public void Save(RateBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate batches. Empty fields match all.</summary>
public sealed record RateBatchQuery(string Segment, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(RateBatch item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<RateBatch> Apply(List<RateBatch> items)
    {
        var matched = Collections.Filter(items, (RateBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate batch store.</summary>
public static class RateBatchStore
{
    public static IRateBatchStore NewMemoryRateBatchStore()
    {
        return new MemoryRateBatchStore(new Dictionary<string, RateBatch>(), new List<string>());
    }

    public static IRateBatchStore NewCachedRateBatchStore(MemoryRateBatchStore inner, int ttl, IClock clock)
    {
        return new CachedRateBatchStore(inner, RateBatchCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RateBatch> FindRateBatches(IRateBatchStore store, RateBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
