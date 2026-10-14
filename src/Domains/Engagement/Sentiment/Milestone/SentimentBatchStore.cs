namespace Acme.Workforce.Engagement.Sentiment.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment batches by code.</summary>
public interface ISentimentBatchStore
{
    SentimentBatch? Find(string code);
    void Save(SentimentBatch item);
    bool Remove(string code);
    List<SentimentBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentBatchStore(Dictionary<string, SentimentBatch> items, List<string> order) : ISentimentBatchStore
{
    public Dictionary<string, SentimentBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentBatch item)
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

    public List<SentimentBatch> List()
    {
        var outValue = new List<SentimentBatch>();
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
public sealed class CachedSentimentBatchStore(MemorySentimentBatchStore inner, SentimentBatchCache cache, int hits, int misses) : ISentimentBatchStore
{
    public MemorySentimentBatchStore Inner { get; set; } = inner;
    public SentimentBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentBatch? Find(string code)
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

    public void Save(SentimentBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment batches. Empty fields match all.</summary>
public sealed record SentimentBatchQuery(string Segment, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentBatch item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<SentimentBatch> Apply(List<SentimentBatch> items)
    {
        var matched = Collections.Filter(items, (SentimentBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment batch store.</summary>
public static class SentimentBatchStore
{
    public static ISentimentBatchStore NewMemorySentimentBatchStore()
    {
        return new MemorySentimentBatchStore(new Dictionary<string, SentimentBatch>(), new List<string>());
    }

    public static ISentimentBatchStore NewCachedSentimentBatchStore(MemorySentimentBatchStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentBatchStore(inner, SentimentBatchCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<SentimentBatch> FindSentimentBatches(ISentimentBatchStore store, SentimentBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
