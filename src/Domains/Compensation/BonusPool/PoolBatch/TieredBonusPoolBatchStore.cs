namespace Acme.Workforce.Compensation.BonusPool.PoolBatch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered bonus pool batches by code.</summary>
public interface ITieredBonusPoolBatchStore
{
    TieredBonusPoolBatch? Find(string code);
    void Save(TieredBonusPoolBatch item);
    bool Remove(string code);
    List<TieredBonusPoolBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredBonusPoolBatchStore(Dictionary<string, TieredBonusPoolBatch> items, List<string> order) : ITieredBonusPoolBatchStore
{
    public Dictionary<string, TieredBonusPoolBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredBonusPoolBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredBonusPoolBatch item)
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

    public List<TieredBonusPoolBatch> List()
    {
        var outValue = new List<TieredBonusPoolBatch>();
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
public sealed class CachedTieredBonusPoolBatchStore(MemoryTieredBonusPoolBatchStore inner, TieredBonusPoolBatchCache cache, int hits, int misses) : ITieredBonusPoolBatchStore
{
    public MemoryTieredBonusPoolBatchStore Inner { get; set; } = inner;
    public TieredBonusPoolBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredBonusPoolBatch? Find(string code)
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

    public void Save(TieredBonusPoolBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredBonusPoolBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered bonus pool batches. Empty fields match all.</summary>
public sealed record TieredBonusPoolBatchQuery(string Name, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(TieredBonusPoolBatch item)
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

    public List<TieredBonusPoolBatch> Apply(List<TieredBonusPoolBatch> items)
    {
        var matched = Collections.Filter(items, (TieredBonusPoolBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered bonus pool batch store.</summary>
public static class TieredBonusPoolBatchStore
{
    public static ITieredBonusPoolBatchStore NewMemoryTieredBonusPoolBatchStore()
    {
        return new MemoryTieredBonusPoolBatchStore(
            new Dictionary<string, TieredBonusPoolBatch>(),
            new List<string>());
    }

    public static ITieredBonusPoolBatchStore NewCachedTieredBonusPoolBatchStore(MemoryTieredBonusPoolBatchStore inner, int ttl, IClock clock)
    {
        return new CachedTieredBonusPoolBatchStore(inner, TieredBonusPoolBatchCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TieredBonusPoolBatch> FindTieredBonusPoolBatches(ITieredBonusPoolBatchStore store, TieredBonusPoolBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
