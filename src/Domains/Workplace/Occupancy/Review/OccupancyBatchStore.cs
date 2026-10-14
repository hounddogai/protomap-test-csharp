namespace Acme.Workforce.Workplace.Occupancy.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy batches by code.</summary>
public interface IOccupancyBatchStore
{
    OccupancyBatch? Find(string code);
    void Save(OccupancyBatch item);
    bool Remove(string code);
    List<OccupancyBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyBatchStore(Dictionary<string, OccupancyBatch> items, List<string> order) : IOccupancyBatchStore
{
    public Dictionary<string, OccupancyBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyBatch item)
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

    public List<OccupancyBatch> List()
    {
        var outValue = new List<OccupancyBatch>();
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
public sealed class CachedOccupancyBatchStore(MemoryOccupancyBatchStore inner, OccupancyBatchCache cache, int hits, int misses) : IOccupancyBatchStore
{
    public MemoryOccupancyBatchStore Inner { get; set; } = inner;
    public OccupancyBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyBatch? Find(string code)
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

    public void Save(OccupancyBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy batches. Empty fields match all.</summary>
public sealed record OccupancyBatchQuery(string Department, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyBatch item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OccupancyBatch> Apply(List<OccupancyBatch> items)
    {
        var matched = Collections.Filter(items, (OccupancyBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy batch store.</summary>
public static class OccupancyBatchStore
{
    public static IOccupancyBatchStore NewMemoryOccupancyBatchStore()
    {
        return new MemoryOccupancyBatchStore(new Dictionary<string, OccupancyBatch>(), new List<string>());
    }

    public static IOccupancyBatchStore NewCachedOccupancyBatchStore(MemoryOccupancyBatchStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyBatchStore(inner, OccupancyBatchCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<OccupancyBatch> FindOccupancyBatches(IOccupancyBatchStore store, OccupancyBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
