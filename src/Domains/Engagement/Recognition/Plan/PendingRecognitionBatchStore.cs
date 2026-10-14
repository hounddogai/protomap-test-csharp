namespace Acme.Workforce.Engagement.Recognition.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending recognition batches by code.</summary>
public interface IPendingRecognitionBatchStore
{
    PendingRecognitionBatch? Find(string code);
    void Save(PendingRecognitionBatch item);
    bool Remove(string code);
    List<PendingRecognitionBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingRecognitionBatchStore(Dictionary<string, PendingRecognitionBatch> items, List<string> order) : IPendingRecognitionBatchStore
{
    public Dictionary<string, PendingRecognitionBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingRecognitionBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingRecognitionBatch item)
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

    public List<PendingRecognitionBatch> List()
    {
        var outValue = new List<PendingRecognitionBatch>();
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
public sealed class CachedPendingRecognitionBatchStore(MemoryPendingRecognitionBatchStore inner, PendingRecognitionBatchCache cache, int hits, int misses) : IPendingRecognitionBatchStore
{
    public MemoryPendingRecognitionBatchStore Inner { get; set; } = inner;
    public PendingRecognitionBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingRecognitionBatch? Find(string code)
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

    public void Save(PendingRecognitionBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingRecognitionBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending recognition batches. Empty fields match all.</summary>
public sealed record PendingRecognitionBatchQuery(string Channel, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(PendingRecognitionBatch item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingRecognitionBatch> Apply(List<PendingRecognitionBatch> items)
    {
        var matched = Collections.Filter(items, (PendingRecognitionBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending recognition batch store.</summary>
public static class PendingRecognitionBatchStore
{
    public static IPendingRecognitionBatchStore NewMemoryPendingRecognitionBatchStore()
    {
        return new MemoryPendingRecognitionBatchStore(
            new Dictionary<string, PendingRecognitionBatch>(),
            new List<string>());
    }

    public static IPendingRecognitionBatchStore NewCachedPendingRecognitionBatchStore(MemoryPendingRecognitionBatchStore inner, int ttl, IClock clock)
    {
        return new CachedPendingRecognitionBatchStore(
            inner,
            PendingRecognitionBatchCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PendingRecognitionBatch> FindPendingRecognitionBatches(IPendingRecognitionBatchStore store, PendingRecognitionBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
