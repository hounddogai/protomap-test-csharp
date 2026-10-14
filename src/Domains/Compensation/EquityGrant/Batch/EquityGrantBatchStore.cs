namespace Acme.Workforce.Compensation.EquityGrant.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant batches by code.</summary>
public interface IEquityGrantBatchStore
{
    EquityGrantBatch? Find(string code);
    void Save(EquityGrantBatch item);
    bool Remove(string code);
    List<EquityGrantBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantBatchStore(Dictionary<string, EquityGrantBatch> items, List<string> order) : IEquityGrantBatchStore
{
    public Dictionary<string, EquityGrantBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantBatch item)
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

    public List<EquityGrantBatch> List()
    {
        var outValue = new List<EquityGrantBatch>();
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
public sealed class CachedEquityGrantBatchStore(MemoryEquityGrantBatchStore inner, EquityGrantBatchCache cache, int hits, int misses) : IEquityGrantBatchStore
{
    public MemoryEquityGrantBatchStore Inner { get; set; } = inner;
    public EquityGrantBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantBatch? Find(string code)
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

    public void Save(EquityGrantBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant batches. Empty fields match all.</summary>
public sealed record EquityGrantBatchQuery(string Title, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantBatch item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantBatch> Apply(List<EquityGrantBatch> items)
    {
        var matched = Collections.Filter(items, (EquityGrantBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant batch store.</summary>
public static class EquityGrantBatchStore
{
    public static IEquityGrantBatchStore NewMemoryEquityGrantBatchStore()
    {
        return new MemoryEquityGrantBatchStore(new Dictionary<string, EquityGrantBatch>(), new List<string>());
    }

    public static IEquityGrantBatchStore NewCachedEquityGrantBatchStore(MemoryEquityGrantBatchStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantBatchStore(inner, EquityGrantBatchCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<EquityGrantBatch> FindEquityGrantBatches(IEquityGrantBatchStore store, EquityGrantBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
