namespace Acme.Workforce.Assets.Laptop.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop batches by code.</summary>
public interface ILaptopBatchStore
{
    LaptopBatch? Find(string code);
    void Save(LaptopBatch item);
    bool Remove(string code);
    List<LaptopBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopBatchStore(Dictionary<string, LaptopBatch> items, List<string> order) : ILaptopBatchStore
{
    public Dictionary<string, LaptopBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopBatch item)
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

    public List<LaptopBatch> List()
    {
        var outValue = new List<LaptopBatch>();
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
public sealed class CachedLaptopBatchStore(MemoryLaptopBatchStore inner, LaptopBatchCache cache, int hits, int misses) : ILaptopBatchStore
{
    public MemoryLaptopBatchStore Inner { get; set; } = inner;
    public LaptopBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopBatch? Find(string code)
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

    public void Save(LaptopBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop batches. Empty fields match all.</summary>
public sealed record LaptopBatchQuery(string Segment, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopBatch item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopBatch> Apply(List<LaptopBatch> items)
    {
        var matched = Collections.Filter(items, (LaptopBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop batch store.</summary>
public static class LaptopBatchStore
{
    public static ILaptopBatchStore NewMemoryLaptopBatchStore()
    {
        return new MemoryLaptopBatchStore(new Dictionary<string, LaptopBatch>(), new List<string>());
    }

    public static ILaptopBatchStore NewCachedLaptopBatchStore(MemoryLaptopBatchStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopBatchStore(inner, LaptopBatchCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LaptopBatch> FindLaptopBatches(ILaptopBatchStore store, LaptopBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
