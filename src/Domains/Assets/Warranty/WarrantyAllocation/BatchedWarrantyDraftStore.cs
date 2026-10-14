namespace Acme.Workforce.Assets.Warranty.WarrantyAllocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched warranty drafts by code.</summary>
public interface IBatchedWarrantyDraftStore
{
    BatchedWarrantyDraft? Find(string code);
    void Save(BatchedWarrantyDraft item);
    bool Remove(string code);
    List<BatchedWarrantyDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedWarrantyDraftStore(Dictionary<string, BatchedWarrantyDraft> items, List<string> order) : IBatchedWarrantyDraftStore
{
    public Dictionary<string, BatchedWarrantyDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedWarrantyDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedWarrantyDraft item)
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

    public List<BatchedWarrantyDraft> List()
    {
        var outValue = new List<BatchedWarrantyDraft>();
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
public sealed class CachedBatchedWarrantyDraftStore(MemoryBatchedWarrantyDraftStore inner, BatchedWarrantyDraftCache cache, int hits, int misses) : IBatchedWarrantyDraftStore
{
    public MemoryBatchedWarrantyDraftStore Inner { get; set; } = inner;
    public BatchedWarrantyDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedWarrantyDraft? Find(string code)
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

    public void Save(BatchedWarrantyDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedWarrantyDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched warranty drafts. Empty fields match all.</summary>
public sealed record BatchedWarrantyDraftQuery(string Title, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedWarrantyDraft item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<BatchedWarrantyDraft> Apply(List<BatchedWarrantyDraft> items)
    {
        var matched = Collections.Filter(items, (BatchedWarrantyDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched warranty draft store.</summary>
public static class BatchedWarrantyDraftStore
{
    public static IBatchedWarrantyDraftStore NewMemoryBatchedWarrantyDraftStore()
    {
        return new MemoryBatchedWarrantyDraftStore(
            new Dictionary<string, BatchedWarrantyDraft>(),
            new List<string>());
    }

    public static IBatchedWarrantyDraftStore NewCachedBatchedWarrantyDraftStore(MemoryBatchedWarrantyDraftStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedWarrantyDraftStore(inner, BatchedWarrantyDraftCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BatchedWarrantyDraft> FindBatchedWarrantyDrafts(IBatchedWarrantyDraftStore store, BatchedWarrantyDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
