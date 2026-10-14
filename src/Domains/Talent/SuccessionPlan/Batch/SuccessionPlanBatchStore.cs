namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan batches by code.</summary>
public interface ISuccessionPlanBatchStore
{
    SuccessionPlanBatch? Find(string code);
    void Save(SuccessionPlanBatch item);
    bool Remove(string code);
    List<SuccessionPlanBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanBatchStore(Dictionary<string, SuccessionPlanBatch> items, List<string> order) : ISuccessionPlanBatchStore
{
    public Dictionary<string, SuccessionPlanBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanBatch item)
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

    public List<SuccessionPlanBatch> List()
    {
        var outValue = new List<SuccessionPlanBatch>();
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
public sealed class CachedSuccessionPlanBatchStore(MemorySuccessionPlanBatchStore inner, SuccessionPlanBatchCache cache, int hits, int misses) : ISuccessionPlanBatchStore
{
    public MemorySuccessionPlanBatchStore Inner { get; set; } = inner;
    public SuccessionPlanBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanBatch? Find(string code)
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

    public void Save(SuccessionPlanBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan batches. Empty fields match all.</summary>
public sealed record SuccessionPlanBatchQuery(string Region, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanBatch item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SuccessionPlanBatch> Apply(List<SuccessionPlanBatch> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan batch store.</summary>
public static class SuccessionPlanBatchStore
{
    public static ISuccessionPlanBatchStore NewMemorySuccessionPlanBatchStore()
    {
        return new MemorySuccessionPlanBatchStore(new Dictionary<string, SuccessionPlanBatch>(), new List<string>());
    }

    public static ISuccessionPlanBatchStore NewCachedSuccessionPlanBatchStore(MemorySuccessionPlanBatchStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanBatchStore(inner, SuccessionPlanBatchCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SuccessionPlanBatch> FindSuccessionPlanBatches(ISuccessionPlanBatchStore store, SuccessionPlanBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
