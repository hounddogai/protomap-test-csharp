namespace Acme.Workforce.Compensation.BonusPool.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool batches by code.</summary>
public interface IBonusPoolBatchStore
{
    BonusPoolBatch? Find(string code);
    void Save(BonusPoolBatch item);
    bool Remove(string code);
    List<BonusPoolBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolBatchStore(Dictionary<string, BonusPoolBatch> items, List<string> order) : IBonusPoolBatchStore
{
    public Dictionary<string, BonusPoolBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolBatch item)
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

    public List<BonusPoolBatch> List()
    {
        var outValue = new List<BonusPoolBatch>();
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
public sealed class CachedBonusPoolBatchStore(MemoryBonusPoolBatchStore inner, BonusPoolBatchCache cache, int hits, int misses) : IBonusPoolBatchStore
{
    public MemoryBonusPoolBatchStore Inner { get; set; } = inner;
    public BonusPoolBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolBatch? Find(string code)
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

    public void Save(BonusPoolBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool batches. Empty fields match all.</summary>
public sealed record BonusPoolBatchQuery(string Segment, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolBatch item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolBatch> Apply(List<BonusPoolBatch> items)
    {
        var matched = Collections.Filter(items, (BonusPoolBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool batch store.</summary>
public static class BonusPoolBatchStore
{
    public static IBonusPoolBatchStore NewMemoryBonusPoolBatchStore()
    {
        return new MemoryBonusPoolBatchStore(new Dictionary<string, BonusPoolBatch>(), new List<string>());
    }

    public static IBonusPoolBatchStore NewCachedBonusPoolBatchStore(MemoryBonusPoolBatchStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolBatchStore(inner, BonusPoolBatchCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BonusPoolBatch> FindBonusPoolBatches(IBonusPoolBatchStore store, BonusPoolBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
