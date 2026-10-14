namespace Acme.Workforce.Talent.SuccessionPlan.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual succession plan batches by code.</summary>
public interface IManualSuccessionPlanBatchStore
{
    ManualSuccessionPlanBatch? Find(string code);
    void Save(ManualSuccessionPlanBatch item);
    bool Remove(string code);
    List<ManualSuccessionPlanBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualSuccessionPlanBatchStore(Dictionary<string, ManualSuccessionPlanBatch> items, List<string> order) : IManualSuccessionPlanBatchStore
{
    public Dictionary<string, ManualSuccessionPlanBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualSuccessionPlanBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualSuccessionPlanBatch item)
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

    public List<ManualSuccessionPlanBatch> List()
    {
        var outValue = new List<ManualSuccessionPlanBatch>();
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
public sealed class CachedManualSuccessionPlanBatchStore(MemoryManualSuccessionPlanBatchStore inner, ManualSuccessionPlanBatchCache cache, int hits, int misses) : IManualSuccessionPlanBatchStore
{
    public MemoryManualSuccessionPlanBatchStore Inner { get; set; } = inner;
    public ManualSuccessionPlanBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualSuccessionPlanBatch? Find(string code)
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

    public void Save(ManualSuccessionPlanBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualSuccessionPlanBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual succession plan batches. Empty fields match all.</summary>
public sealed record ManualSuccessionPlanBatchQuery(string Title, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ManualSuccessionPlanBatch item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualSuccessionPlanBatch> Apply(List<ManualSuccessionPlanBatch> items)
    {
        var matched = Collections.Filter(items, (ManualSuccessionPlanBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual succession plan batch store.</summary>
public static class ManualSuccessionPlanBatchStore
{
    public static IManualSuccessionPlanBatchStore NewMemoryManualSuccessionPlanBatchStore()
    {
        return new MemoryManualSuccessionPlanBatchStore(
            new Dictionary<string, ManualSuccessionPlanBatch>(),
            new List<string>());
    }

    public static IManualSuccessionPlanBatchStore NewCachedManualSuccessionPlanBatchStore(MemoryManualSuccessionPlanBatchStore inner, int ttl, IClock clock)
    {
        return new CachedManualSuccessionPlanBatchStore(
            inner,
            ManualSuccessionPlanBatchCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ManualSuccessionPlanBatch> FindManualSuccessionPlanBatches(IManualSuccessionPlanBatchStore store, ManualSuccessionPlanBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
