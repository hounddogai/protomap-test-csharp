namespace Acme.Workforce.Engagement.Sentiment.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment summaries by code.</summary>
public interface ISentimentSummaryStore
{
    SentimentSummary? Find(string code);
    void Save(SentimentSummary item);
    bool Remove(string code);
    List<SentimentSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentSummaryStore(Dictionary<string, SentimentSummary> items, List<string> order) : ISentimentSummaryStore
{
    public Dictionary<string, SentimentSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentSummary item)
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

    public List<SentimentSummary> List()
    {
        var outValue = new List<SentimentSummary>();
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
public sealed class CachedSentimentSummaryStore(MemorySentimentSummaryStore inner, SentimentSummaryCache cache, int hits, int misses) : ISentimentSummaryStore
{
    public MemorySentimentSummaryStore Inner { get; set; } = inner;
    public SentimentSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentSummary? Find(string code)
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

    public void Save(SentimentSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment summaries. Empty fields match all.</summary>
public sealed record SentimentSummaryQuery(string Title, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentSummary item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SentimentSummary> Apply(List<SentimentSummary> items)
    {
        var matched = Collections.Filter(items, (SentimentSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment summary store.</summary>
public static class SentimentSummaryStore
{
    public static ISentimentSummaryStore NewMemorySentimentSummaryStore()
    {
        return new MemorySentimentSummaryStore(new Dictionary<string, SentimentSummary>(), new List<string>());
    }

    public static ISentimentSummaryStore NewCachedSentimentSummaryStore(MemorySentimentSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentSummaryStore(inner, SentimentSummaryCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<SentimentSummary> FindSentimentSummaries(ISentimentSummaryStore store, SentimentSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
