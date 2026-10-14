namespace Acme.Workforce.Orgdesign.Span.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual span batches by code.</summary>
public interface IManualSpanBatchStore
{
    ManualSpanBatch? Find(string code);
    void Save(ManualSpanBatch item);
    bool Remove(string code);
    List<ManualSpanBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualSpanBatchStore(Dictionary<string, ManualSpanBatch> items, List<string> order) : IManualSpanBatchStore
{
    public Dictionary<string, ManualSpanBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualSpanBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualSpanBatch item)
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

    public List<ManualSpanBatch> List()
    {
        var outValue = new List<ManualSpanBatch>();
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
public sealed class CachedManualSpanBatchStore(MemoryManualSpanBatchStore inner, ManualSpanBatchCache cache, int hits, int misses) : IManualSpanBatchStore
{
    public MemoryManualSpanBatchStore Inner { get; set; } = inner;
    public ManualSpanBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualSpanBatch? Find(string code)
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

    public void Save(ManualSpanBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualSpanBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual span batches. Empty fields match all.</summary>
public sealed record ManualSpanBatchQuery(string Channel, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(ManualSpanBatch item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualSpanBatch> Apply(List<ManualSpanBatch> items)
    {
        var matched = Collections.Filter(items, (ManualSpanBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual span batch store.</summary>
public static class ManualSpanBatchStore
{
    public static IManualSpanBatchStore NewMemoryManualSpanBatchStore()
    {
        return new MemoryManualSpanBatchStore(new Dictionary<string, ManualSpanBatch>(), new List<string>());
    }

    public static IManualSpanBatchStore NewCachedManualSpanBatchStore(MemoryManualSpanBatchStore inner, int ttl, IClock clock)
    {
        return new CachedManualSpanBatchStore(inner, ManualSpanBatchCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ManualSpanBatch> FindManualSpanBatches(IManualSpanBatchStore store, ManualSpanBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
