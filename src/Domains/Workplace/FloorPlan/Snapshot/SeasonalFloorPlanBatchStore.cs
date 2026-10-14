namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal floor plan batches by code.</summary>
public interface ISeasonalFloorPlanBatchStore
{
    SeasonalFloorPlanBatch? Find(string code);
    void Save(SeasonalFloorPlanBatch item);
    bool Remove(string code);
    List<SeasonalFloorPlanBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalFloorPlanBatchStore(Dictionary<string, SeasonalFloorPlanBatch> items, List<string> order) : ISeasonalFloorPlanBatchStore
{
    public Dictionary<string, SeasonalFloorPlanBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalFloorPlanBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalFloorPlanBatch item)
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

    public List<SeasonalFloorPlanBatch> List()
    {
        var outValue = new List<SeasonalFloorPlanBatch>();
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
public sealed class CachedSeasonalFloorPlanBatchStore(MemorySeasonalFloorPlanBatchStore inner, SeasonalFloorPlanBatchCache cache, int hits, int misses) : ISeasonalFloorPlanBatchStore
{
    public MemorySeasonalFloorPlanBatchStore Inner { get; set; } = inner;
    public SeasonalFloorPlanBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalFloorPlanBatch? Find(string code)
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

    public void Save(SeasonalFloorPlanBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalFloorPlanBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal floor plan batches. Empty fields match all.</summary>
public sealed record SeasonalFloorPlanBatchQuery(string Name, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalFloorPlanBatch item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalFloorPlanBatch> Apply(List<SeasonalFloorPlanBatch> items)
    {
        var matched = Collections.Filter(items, (SeasonalFloorPlanBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal floor plan batch store.</summary>
public static class SeasonalFloorPlanBatchStore
{
    public static ISeasonalFloorPlanBatchStore NewMemorySeasonalFloorPlanBatchStore()
    {
        return new MemorySeasonalFloorPlanBatchStore(
            new Dictionary<string, SeasonalFloorPlanBatch>(),
            new List<string>());
    }

    public static ISeasonalFloorPlanBatchStore NewCachedSeasonalFloorPlanBatchStore(MemorySeasonalFloorPlanBatchStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalFloorPlanBatchStore(
            inner,
            SeasonalFloorPlanBatchCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalFloorPlanBatch> FindSeasonalFloorPlanBatches(ISeasonalFloorPlanBatchStore store, SeasonalFloorPlanBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
