namespace Acme.Workforce.Orgdesign.Headcount.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount batches by code.</summary>
public interface IHeadcountBatchStore
{
    HeadcountBatch? Find(string code);
    void Save(HeadcountBatch item);
    bool Remove(string code);
    List<HeadcountBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountBatchStore(Dictionary<string, HeadcountBatch> items, List<string> order) : IHeadcountBatchStore
{
    public Dictionary<string, HeadcountBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountBatch item)
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

    public List<HeadcountBatch> List()
    {
        var outValue = new List<HeadcountBatch>();
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
public sealed class CachedHeadcountBatchStore(MemoryHeadcountBatchStore inner, HeadcountBatchCache cache, int hits, int misses) : IHeadcountBatchStore
{
    public MemoryHeadcountBatchStore Inner { get; set; } = inner;
    public HeadcountBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountBatch? Find(string code)
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

    public void Save(HeadcountBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount batches. Empty fields match all.</summary>
public sealed record HeadcountBatchQuery(string Department, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountBatch item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<HeadcountBatch> Apply(List<HeadcountBatch> items)
    {
        var matched = Collections.Filter(items, (HeadcountBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount batch store.</summary>
public static class HeadcountBatchStore
{
    public static IHeadcountBatchStore NewMemoryHeadcountBatchStore()
    {
        return new MemoryHeadcountBatchStore(new Dictionary<string, HeadcountBatch>(), new List<string>());
    }

    public static IHeadcountBatchStore NewCachedHeadcountBatchStore(MemoryHeadcountBatchStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountBatchStore(inner, HeadcountBatchCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<HeadcountBatch> FindHeadcountBatches(IHeadcountBatchStore store, HeadcountBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
