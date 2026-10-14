namespace Acme.Workforce.Engagement.ActionPlan.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered action plan batches by code.</summary>
public interface ITieredActionPlanBatchStore
{
    TieredActionPlanBatch? Find(string code);
    void Save(TieredActionPlanBatch item);
    bool Remove(string code);
    List<TieredActionPlanBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredActionPlanBatchStore(Dictionary<string, TieredActionPlanBatch> items, List<string> order) : ITieredActionPlanBatchStore
{
    public Dictionary<string, TieredActionPlanBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredActionPlanBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredActionPlanBatch item)
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

    public List<TieredActionPlanBatch> List()
    {
        var outValue = new List<TieredActionPlanBatch>();
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
public sealed class CachedTieredActionPlanBatchStore(MemoryTieredActionPlanBatchStore inner, TieredActionPlanBatchCache cache, int hits, int misses) : ITieredActionPlanBatchStore
{
    public MemoryTieredActionPlanBatchStore Inner { get; set; } = inner;
    public TieredActionPlanBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredActionPlanBatch? Find(string code)
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

    public void Save(TieredActionPlanBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredActionPlanBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered action plan batches. Empty fields match all.</summary>
public sealed record TieredActionPlanBatchQuery(string Channel, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(TieredActionPlanBatch item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredActionPlanBatch> Apply(List<TieredActionPlanBatch> items)
    {
        var matched = Collections.Filter(items, (TieredActionPlanBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered action plan batch store.</summary>
public static class TieredActionPlanBatchStore
{
    public static ITieredActionPlanBatchStore NewMemoryTieredActionPlanBatchStore()
    {
        return new MemoryTieredActionPlanBatchStore(
            new Dictionary<string, TieredActionPlanBatch>(),
            new List<string>());
    }

    public static ITieredActionPlanBatchStore NewCachedTieredActionPlanBatchStore(MemoryTieredActionPlanBatchStore inner, int ttl, IClock clock)
    {
        return new CachedTieredActionPlanBatchStore(inner, TieredActionPlanBatchCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TieredActionPlanBatch> FindTieredActionPlanBatches(ITieredActionPlanBatchStore store, TieredActionPlanBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
