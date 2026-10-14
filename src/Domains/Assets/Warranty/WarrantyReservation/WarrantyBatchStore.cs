namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty batches by code.</summary>
public interface IWarrantyBatchStore
{
    WarrantyBatch? Find(string code);
    void Save(WarrantyBatch item);
    bool Remove(string code);
    List<WarrantyBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyBatchStore(Dictionary<string, WarrantyBatch> items, List<string> order) : IWarrantyBatchStore
{
    public Dictionary<string, WarrantyBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyBatch item)
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

    public List<WarrantyBatch> List()
    {
        var outValue = new List<WarrantyBatch>();
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
public sealed class CachedWarrantyBatchStore(MemoryWarrantyBatchStore inner, WarrantyBatchCache cache, int hits, int misses) : IWarrantyBatchStore
{
    public MemoryWarrantyBatchStore Inner { get; set; } = inner;
    public WarrantyBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyBatch? Find(string code)
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

    public void Save(WarrantyBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty batches. Empty fields match all.</summary>
public sealed record WarrantyBatchQuery(string Locale, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyBatch item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyBatch> Apply(List<WarrantyBatch> items)
    {
        var matched = Collections.Filter(items, (WarrantyBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty batch store.</summary>
public static class WarrantyBatchStore
{
    public static IWarrantyBatchStore NewMemoryWarrantyBatchStore()
    {
        return new MemoryWarrantyBatchStore(new Dictionary<string, WarrantyBatch>(), new List<string>());
    }

    public static IWarrantyBatchStore NewCachedWarrantyBatchStore(MemoryWarrantyBatchStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyBatchStore(inner, WarrantyBatchCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<WarrantyBatch> FindWarrantyBatches(IWarrantyBatchStore store, WarrantyBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
