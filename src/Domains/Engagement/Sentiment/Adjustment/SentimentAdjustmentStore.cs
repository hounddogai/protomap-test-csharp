namespace Acme.Workforce.Engagement.Sentiment.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment adjustments by code.</summary>
public interface ISentimentAdjustmentStore
{
    SentimentAdjustment? Find(string code);
    void Save(SentimentAdjustment item);
    bool Remove(string code);
    List<SentimentAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentAdjustmentStore(Dictionary<string, SentimentAdjustment> items, List<string> order) : ISentimentAdjustmentStore
{
    public Dictionary<string, SentimentAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentAdjustment item)
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

    public List<SentimentAdjustment> List()
    {
        var outValue = new List<SentimentAdjustment>();
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
public sealed class CachedSentimentAdjustmentStore(MemorySentimentAdjustmentStore inner, SentimentAdjustmentCache cache, int hits, int misses) : ISentimentAdjustmentStore
{
    public MemorySentimentAdjustmentStore Inner { get; set; } = inner;
    public SentimentAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentAdjustment? Find(string code)
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

    public void Save(SentimentAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment adjustments. Empty fields match all.</summary>
public sealed record SentimentAdjustmentQuery(string Title, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentAdjustment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<SentimentAdjustment> Apply(List<SentimentAdjustment> items)
    {
        var matched = Collections.Filter(items, (SentimentAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment adjustment store.</summary>
public static class SentimentAdjustmentStore
{
    public static ISentimentAdjustmentStore NewMemorySentimentAdjustmentStore()
    {
        return new MemorySentimentAdjustmentStore(new Dictionary<string, SentimentAdjustment>(), new List<string>());
    }

    public static ISentimentAdjustmentStore NewCachedSentimentAdjustmentStore(MemorySentimentAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentAdjustmentStore(inner, SentimentAdjustmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SentimentAdjustment> FindSentimentAdjustments(ISentimentAdjustmentStore store, SentimentAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
