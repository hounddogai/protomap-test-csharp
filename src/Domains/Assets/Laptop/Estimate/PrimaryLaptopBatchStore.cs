namespace Acme.Workforce.Assets.Laptop.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary laptop batches by code.</summary>
public interface IPrimaryLaptopBatchStore
{
    PrimaryLaptopBatch? Find(string code);
    void Save(PrimaryLaptopBatch item);
    bool Remove(string code);
    List<PrimaryLaptopBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryLaptopBatchStore(Dictionary<string, PrimaryLaptopBatch> items, List<string> order) : IPrimaryLaptopBatchStore
{
    public Dictionary<string, PrimaryLaptopBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryLaptopBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryLaptopBatch item)
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

    public List<PrimaryLaptopBatch> List()
    {
        var outValue = new List<PrimaryLaptopBatch>();
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
public sealed class CachedPrimaryLaptopBatchStore(MemoryPrimaryLaptopBatchStore inner, PrimaryLaptopBatchCache cache, int hits, int misses) : IPrimaryLaptopBatchStore
{
    public MemoryPrimaryLaptopBatchStore Inner { get; set; } = inner;
    public PrimaryLaptopBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryLaptopBatch? Find(string code)
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

    public void Save(PrimaryLaptopBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryLaptopBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary laptop batches. Empty fields match all.</summary>
public sealed record PrimaryLaptopBatchQuery(string Unit, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryLaptopBatch item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryLaptopBatch> Apply(List<PrimaryLaptopBatch> items)
    {
        var matched = Collections.Filter(items, (PrimaryLaptopBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary laptop batch store.</summary>
public static class PrimaryLaptopBatchStore
{
    public static IPrimaryLaptopBatchStore NewMemoryPrimaryLaptopBatchStore()
    {
        return new MemoryPrimaryLaptopBatchStore(new Dictionary<string, PrimaryLaptopBatch>(), new List<string>());
    }

    public static IPrimaryLaptopBatchStore NewCachedPrimaryLaptopBatchStore(MemoryPrimaryLaptopBatchStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryLaptopBatchStore(inner, PrimaryLaptopBatchCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PrimaryLaptopBatch> FindPrimaryLaptopBatches(IPrimaryLaptopBatchStore store, PrimaryLaptopBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
