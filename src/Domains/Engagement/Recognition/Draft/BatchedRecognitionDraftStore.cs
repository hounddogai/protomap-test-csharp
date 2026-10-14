namespace Acme.Workforce.Engagement.Recognition.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched recognition drafts by code.</summary>
public interface IBatchedRecognitionDraftStore
{
    BatchedRecognitionDraft? Find(string code);
    void Save(BatchedRecognitionDraft item);
    bool Remove(string code);
    List<BatchedRecognitionDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedRecognitionDraftStore(Dictionary<string, BatchedRecognitionDraft> items, List<string> order) : IBatchedRecognitionDraftStore
{
    public Dictionary<string, BatchedRecognitionDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedRecognitionDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedRecognitionDraft item)
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

    public List<BatchedRecognitionDraft> List()
    {
        var outValue = new List<BatchedRecognitionDraft>();
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
public sealed class CachedBatchedRecognitionDraftStore(MemoryBatchedRecognitionDraftStore inner, BatchedRecognitionDraftCache cache, int hits, int misses) : IBatchedRecognitionDraftStore
{
    public MemoryBatchedRecognitionDraftStore Inner { get; set; } = inner;
    public BatchedRecognitionDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedRecognitionDraft? Find(string code)
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

    public void Save(BatchedRecognitionDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedRecognitionDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched recognition drafts. Empty fields match all.</summary>
public sealed record BatchedRecognitionDraftQuery(string Name, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedRecognitionDraft item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedRecognitionDraft> Apply(List<BatchedRecognitionDraft> items)
    {
        var matched = Collections.Filter(items, (BatchedRecognitionDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched recognition draft store.</summary>
public static class BatchedRecognitionDraftStore
{
    public static IBatchedRecognitionDraftStore NewMemoryBatchedRecognitionDraftStore()
    {
        return new MemoryBatchedRecognitionDraftStore(
            new Dictionary<string, BatchedRecognitionDraft>(),
            new List<string>());
    }

    public static IBatchedRecognitionDraftStore NewCachedBatchedRecognitionDraftStore(MemoryBatchedRecognitionDraftStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedRecognitionDraftStore(
            inner,
            BatchedRecognitionDraftCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<BatchedRecognitionDraft> FindBatchedRecognitionDrafts(IBatchedRecognitionDraftStore store, BatchedRecognitionDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
