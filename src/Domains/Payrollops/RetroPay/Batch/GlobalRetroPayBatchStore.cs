namespace Acme.Workforce.Payrollops.RetroPay.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global retro pay batches by code.</summary>
public interface IGlobalRetroPayBatchStore
{
    GlobalRetroPayBatch? Find(string code);
    void Save(GlobalRetroPayBatch item);
    bool Remove(string code);
    List<GlobalRetroPayBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalRetroPayBatchStore(Dictionary<string, GlobalRetroPayBatch> items, List<string> order) : IGlobalRetroPayBatchStore
{
    public Dictionary<string, GlobalRetroPayBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalRetroPayBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalRetroPayBatch item)
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

    public List<GlobalRetroPayBatch> List()
    {
        var outValue = new List<GlobalRetroPayBatch>();
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
public sealed class CachedGlobalRetroPayBatchStore(MemoryGlobalRetroPayBatchStore inner, GlobalRetroPayBatchCache cache, int hits, int misses) : IGlobalRetroPayBatchStore
{
    public MemoryGlobalRetroPayBatchStore Inner { get; set; } = inner;
    public GlobalRetroPayBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalRetroPayBatch? Find(string code)
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

    public void Save(GlobalRetroPayBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalRetroPayBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global retro pay batches. Empty fields match all.</summary>
public sealed record GlobalRetroPayBatchQuery(string Name, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalRetroPayBatch item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalRetroPayBatch> Apply(List<GlobalRetroPayBatch> items)
    {
        var matched = Collections.Filter(items, (GlobalRetroPayBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global retro pay batch store.</summary>
public static class GlobalRetroPayBatchStore
{
    public static IGlobalRetroPayBatchStore NewMemoryGlobalRetroPayBatchStore()
    {
        return new MemoryGlobalRetroPayBatchStore(new Dictionary<string, GlobalRetroPayBatch>(), new List<string>());
    }

    public static IGlobalRetroPayBatchStore NewCachedGlobalRetroPayBatchStore(MemoryGlobalRetroPayBatchStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalRetroPayBatchStore(inner, GlobalRetroPayBatchCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<GlobalRetroPayBatch> FindGlobalRetroPayBatches(IGlobalRetroPayBatchStore store, GlobalRetroPayBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
