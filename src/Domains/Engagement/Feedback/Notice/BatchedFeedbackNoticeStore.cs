namespace Acme.Workforce.Engagement.Feedback.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched feedback notices by code.</summary>
public interface IBatchedFeedbackNoticeStore
{
    BatchedFeedbackNotice? Find(string code);
    void Save(BatchedFeedbackNotice item);
    bool Remove(string code);
    List<BatchedFeedbackNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedFeedbackNoticeStore(Dictionary<string, BatchedFeedbackNotice> items, List<string> order) : IBatchedFeedbackNoticeStore
{
    public Dictionary<string, BatchedFeedbackNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedFeedbackNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedFeedbackNotice item)
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

    public List<BatchedFeedbackNotice> List()
    {
        var outValue = new List<BatchedFeedbackNotice>();
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
public sealed class CachedBatchedFeedbackNoticeStore(MemoryBatchedFeedbackNoticeStore inner, BatchedFeedbackNoticeCache cache, int hits, int misses) : IBatchedFeedbackNoticeStore
{
    public MemoryBatchedFeedbackNoticeStore Inner { get; set; } = inner;
    public BatchedFeedbackNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedFeedbackNotice? Find(string code)
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

    public void Save(BatchedFeedbackNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedFeedbackNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched feedback notices. Empty fields match all.</summary>
public sealed record BatchedFeedbackNoticeQuery(string Label, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedFeedbackNotice item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedFeedbackNotice> Apply(List<BatchedFeedbackNotice> items)
    {
        var matched = Collections.Filter(items, (BatchedFeedbackNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched feedback notice store.</summary>
public static class BatchedFeedbackNoticeStore
{
    public static IBatchedFeedbackNoticeStore NewMemoryBatchedFeedbackNoticeStore()
    {
        return new MemoryBatchedFeedbackNoticeStore(
            new Dictionary<string, BatchedFeedbackNotice>(),
            new List<string>());
    }

    public static IBatchedFeedbackNoticeStore NewCachedBatchedFeedbackNoticeStore(MemoryBatchedFeedbackNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedFeedbackNoticeStore(inner, BatchedFeedbackNoticeCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BatchedFeedbackNotice> FindBatchedFeedbackNotices(IBatchedFeedbackNoticeStore store, BatchedFeedbackNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
