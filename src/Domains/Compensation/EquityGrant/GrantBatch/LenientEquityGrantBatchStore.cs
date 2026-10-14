namespace Acme.Workforce.Compensation.EquityGrant.GrantBatch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient equity grant batches by code.</summary>
public interface ILenientEquityGrantBatchStore
{
    LenientEquityGrantBatch? Find(string code);
    void Save(LenientEquityGrantBatch item);
    bool Remove(string code);
    List<LenientEquityGrantBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientEquityGrantBatchStore(Dictionary<string, LenientEquityGrantBatch> items, List<string> order) : ILenientEquityGrantBatchStore
{
    public Dictionary<string, LenientEquityGrantBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientEquityGrantBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientEquityGrantBatch item)
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

    public List<LenientEquityGrantBatch> List()
    {
        var outValue = new List<LenientEquityGrantBatch>();
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
public sealed class CachedLenientEquityGrantBatchStore(MemoryLenientEquityGrantBatchStore inner, LenientEquityGrantBatchCache cache, int hits, int misses) : ILenientEquityGrantBatchStore
{
    public MemoryLenientEquityGrantBatchStore Inner { get; set; } = inner;
    public LenientEquityGrantBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientEquityGrantBatch? Find(string code)
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

    public void Save(LenientEquityGrantBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientEquityGrantBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient equity grant batches. Empty fields match all.</summary>
public sealed record LenientEquityGrantBatchQuery(string Unit, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(LenientEquityGrantBatch item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientEquityGrantBatch> Apply(List<LenientEquityGrantBatch> items)
    {
        var matched = Collections.Filter(items, (LenientEquityGrantBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient equity grant batch store.</summary>
public static class LenientEquityGrantBatchStore
{
    public static ILenientEquityGrantBatchStore NewMemoryLenientEquityGrantBatchStore()
    {
        return new MemoryLenientEquityGrantBatchStore(
            new Dictionary<string, LenientEquityGrantBatch>(),
            new List<string>());
    }

    public static ILenientEquityGrantBatchStore NewCachedLenientEquityGrantBatchStore(MemoryLenientEquityGrantBatchStore inner, int ttl, IClock clock)
    {
        return new CachedLenientEquityGrantBatchStore(
            inner,
            LenientEquityGrantBatchCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LenientEquityGrantBatch> FindLenientEquityGrantBatches(ILenientEquityGrantBatchStore store, LenientEquityGrantBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
