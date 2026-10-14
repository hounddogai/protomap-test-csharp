namespace Acme.Workforce.Workplace.Visitor.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary visitor batches by code.</summary>
public interface IPrimaryVisitorBatchStore
{
    PrimaryVisitorBatch? Find(string code);
    void Save(PrimaryVisitorBatch item);
    bool Remove(string code);
    List<PrimaryVisitorBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryVisitorBatchStore(Dictionary<string, PrimaryVisitorBatch> items, List<string> order) : IPrimaryVisitorBatchStore
{
    public Dictionary<string, PrimaryVisitorBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryVisitorBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryVisitorBatch item)
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

    public List<PrimaryVisitorBatch> List()
    {
        var outValue = new List<PrimaryVisitorBatch>();
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
public sealed class CachedPrimaryVisitorBatchStore(MemoryPrimaryVisitorBatchStore inner, PrimaryVisitorBatchCache cache, int hits, int misses) : IPrimaryVisitorBatchStore
{
    public MemoryPrimaryVisitorBatchStore Inner { get; set; } = inner;
    public PrimaryVisitorBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryVisitorBatch? Find(string code)
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

    public void Save(PrimaryVisitorBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryVisitorBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary visitor batches. Empty fields match all.</summary>
public sealed record PrimaryVisitorBatchQuery(string Label, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryVisitorBatch item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<PrimaryVisitorBatch> Apply(List<PrimaryVisitorBatch> items)
    {
        var matched = Collections.Filter(items, (PrimaryVisitorBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary visitor batch store.</summary>
public static class PrimaryVisitorBatchStore
{
    public static IPrimaryVisitorBatchStore NewMemoryPrimaryVisitorBatchStore()
    {
        return new MemoryPrimaryVisitorBatchStore(new Dictionary<string, PrimaryVisitorBatch>(), new List<string>());
    }

    public static IPrimaryVisitorBatchStore NewCachedPrimaryVisitorBatchStore(MemoryPrimaryVisitorBatchStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryVisitorBatchStore(inner, PrimaryVisitorBatchCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PrimaryVisitorBatch> FindPrimaryVisitorBatches(IPrimaryVisitorBatchStore store, PrimaryVisitorBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
