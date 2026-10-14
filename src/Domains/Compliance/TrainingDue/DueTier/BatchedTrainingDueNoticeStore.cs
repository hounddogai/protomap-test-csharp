namespace Acme.Workforce.Compliance.TrainingDue.DueTier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched training due notices by code.</summary>
public interface IBatchedTrainingDueNoticeStore
{
    BatchedTrainingDueNotice? Find(string code);
    void Save(BatchedTrainingDueNotice item);
    bool Remove(string code);
    List<BatchedTrainingDueNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedTrainingDueNoticeStore(Dictionary<string, BatchedTrainingDueNotice> items, List<string> order) : IBatchedTrainingDueNoticeStore
{
    public Dictionary<string, BatchedTrainingDueNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedTrainingDueNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedTrainingDueNotice item)
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

    public List<BatchedTrainingDueNotice> List()
    {
        var outValue = new List<BatchedTrainingDueNotice>();
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
public sealed class CachedBatchedTrainingDueNoticeStore(MemoryBatchedTrainingDueNoticeStore inner, BatchedTrainingDueNoticeCache cache, int hits, int misses) : IBatchedTrainingDueNoticeStore
{
    public MemoryBatchedTrainingDueNoticeStore Inner { get; set; } = inner;
    public BatchedTrainingDueNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedTrainingDueNotice? Find(string code)
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

    public void Save(BatchedTrainingDueNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedTrainingDueNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched training due notices. Empty fields match all.</summary>
public sealed record BatchedTrainingDueNoticeQuery(string Category, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedTrainingDueNotice item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedTrainingDueNotice> Apply(List<BatchedTrainingDueNotice> items)
    {
        var matched = Collections.Filter(items, (BatchedTrainingDueNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched training due notice store.</summary>
public static class BatchedTrainingDueNoticeStore
{
    public static IBatchedTrainingDueNoticeStore NewMemoryBatchedTrainingDueNoticeStore()
    {
        return new MemoryBatchedTrainingDueNoticeStore(
            new Dictionary<string, BatchedTrainingDueNotice>(),
            new List<string>());
    }

    public static IBatchedTrainingDueNoticeStore NewCachedBatchedTrainingDueNoticeStore(MemoryBatchedTrainingDueNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedTrainingDueNoticeStore(
            inner,
            BatchedTrainingDueNoticeCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<BatchedTrainingDueNotice> FindBatchedTrainingDueNotices(IBatchedTrainingDueNoticeStore store, BatchedTrainingDueNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
