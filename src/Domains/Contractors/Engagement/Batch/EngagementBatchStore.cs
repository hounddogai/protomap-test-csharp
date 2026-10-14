namespace Acme.Workforce.Contractors.Engagement.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps engagement batches by code.</summary>
public interface IEngagementBatchStore
{
    EngagementBatch? Find(string code);
    void Save(EngagementBatch item);
    bool Remove(string code);
    List<EngagementBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEngagementBatchStore(Dictionary<string, EngagementBatch> items, List<string> order) : IEngagementBatchStore
{
    public Dictionary<string, EngagementBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EngagementBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EngagementBatch item)
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

    public List<EngagementBatch> List()
    {
        var outValue = new List<EngagementBatch>();
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
public sealed class CachedEngagementBatchStore(MemoryEngagementBatchStore inner, EngagementBatchCache cache, int hits, int misses) : IEngagementBatchStore
{
    public MemoryEngagementBatchStore Inner { get; set; } = inner;
    public EngagementBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EngagementBatch? Find(string code)
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

    public void Save(EngagementBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EngagementBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects engagement batches. Empty fields match all.</summary>
public sealed record EngagementBatchQuery(string Label, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(EngagementBatch item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EngagementBatch> Apply(List<EngagementBatch> items)
    {
        var matched = Collections.Filter(items, (EngagementBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of engagement batch store.</summary>
public static class EngagementBatchStore
{
    public static IEngagementBatchStore NewMemoryEngagementBatchStore()
    {
        return new MemoryEngagementBatchStore(new Dictionary<string, EngagementBatch>(), new List<string>());
    }

    public static IEngagementBatchStore NewCachedEngagementBatchStore(MemoryEngagementBatchStore inner, int ttl, IClock clock)
    {
        return new CachedEngagementBatchStore(inner, EngagementBatchCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<EngagementBatch> FindEngagementBatches(IEngagementBatchStore store, EngagementBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
