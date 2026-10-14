namespace Acme.Workforce.Workplace.Visitor.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor batches by code.</summary>
public interface IVisitorBatchStore
{
    VisitorBatch? Find(string code);
    void Save(VisitorBatch item);
    bool Remove(string code);
    List<VisitorBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorBatchStore(Dictionary<string, VisitorBatch> items, List<string> order) : IVisitorBatchStore
{
    public Dictionary<string, VisitorBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorBatch item)
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

    public List<VisitorBatch> List()
    {
        var outValue = new List<VisitorBatch>();
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
public sealed class CachedVisitorBatchStore(MemoryVisitorBatchStore inner, VisitorBatchCache cache, int hits, int misses) : IVisitorBatchStore
{
    public MemoryVisitorBatchStore Inner { get; set; } = inner;
    public VisitorBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorBatch? Find(string code)
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

    public void Save(VisitorBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor batches. Empty fields match all.</summary>
public sealed record VisitorBatchQuery(string Title, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorBatch item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorBatch> Apply(List<VisitorBatch> items)
    {
        var matched = Collections.Filter(items, (VisitorBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor batch store.</summary>
public static class VisitorBatchStore
{
    public static IVisitorBatchStore NewMemoryVisitorBatchStore()
    {
        return new MemoryVisitorBatchStore(new Dictionary<string, VisitorBatch>(), new List<string>());
    }

    public static IVisitorBatchStore NewCachedVisitorBatchStore(MemoryVisitorBatchStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorBatchStore(inner, VisitorBatchCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<VisitorBatch> FindVisitorBatches(IVisitorBatchStore store, VisitorBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
