namespace Acme.Workforce.Absence.Accrual.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual batches by code.</summary>
public interface IAccrualBatchStore
{
    AccrualBatch? Find(string code);
    void Save(AccrualBatch item);
    bool Remove(string code);
    List<AccrualBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualBatchStore(Dictionary<string, AccrualBatch> items, List<string> order) : IAccrualBatchStore
{
    public Dictionary<string, AccrualBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualBatch item)
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

    public List<AccrualBatch> List()
    {
        var outValue = new List<AccrualBatch>();
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
public sealed class CachedAccrualBatchStore(MemoryAccrualBatchStore inner, AccrualBatchCache cache, int hits, int misses) : IAccrualBatchStore
{
    public MemoryAccrualBatchStore Inner { get; set; } = inner;
    public AccrualBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualBatch? Find(string code)
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

    public void Save(AccrualBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual batches. Empty fields match all.</summary>
public sealed record AccrualBatchQuery(string Country, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualBatch item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccrualBatch> Apply(List<AccrualBatch> items)
    {
        var matched = Collections.Filter(items, (AccrualBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual batch store.</summary>
public static class AccrualBatchStore
{
    public static IAccrualBatchStore NewMemoryAccrualBatchStore()
    {
        return new MemoryAccrualBatchStore(new Dictionary<string, AccrualBatch>(), new List<string>());
    }

    public static IAccrualBatchStore NewCachedAccrualBatchStore(MemoryAccrualBatchStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualBatchStore(inner, AccrualBatchCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AccrualBatch> FindAccrualBatches(IAccrualBatchStore store, AccrualBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
