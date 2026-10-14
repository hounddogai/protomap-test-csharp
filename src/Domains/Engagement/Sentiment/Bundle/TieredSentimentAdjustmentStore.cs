namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered sentiment adjustments by code.</summary>
public interface ITieredSentimentAdjustmentStore
{
    TieredSentimentAdjustment? Find(string code);
    void Save(TieredSentimentAdjustment item);
    bool Remove(string code);
    List<TieredSentimentAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredSentimentAdjustmentStore(Dictionary<string, TieredSentimentAdjustment> items, List<string> order) : ITieredSentimentAdjustmentStore
{
    public Dictionary<string, TieredSentimentAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredSentimentAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredSentimentAdjustment item)
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

    public List<TieredSentimentAdjustment> List()
    {
        var outValue = new List<TieredSentimentAdjustment>();
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
public sealed class CachedTieredSentimentAdjustmentStore(MemoryTieredSentimentAdjustmentStore inner, TieredSentimentAdjustmentCache cache, int hits, int misses) : ITieredSentimentAdjustmentStore
{
    public MemoryTieredSentimentAdjustmentStore Inner { get; set; } = inner;
    public TieredSentimentAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredSentimentAdjustment? Find(string code)
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

    public void Save(TieredSentimentAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredSentimentAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered sentiment adjustments. Empty fields match all.</summary>
public sealed record TieredSentimentAdjustmentQuery(string Label, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(TieredSentimentAdjustment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredSentimentAdjustment> Apply(List<TieredSentimentAdjustment> items)
    {
        var matched = Collections.Filter(items, (TieredSentimentAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered sentiment adjustment store.</summary>
public static class TieredSentimentAdjustmentStore
{
    public static ITieredSentimentAdjustmentStore NewMemoryTieredSentimentAdjustmentStore()
    {
        return new MemoryTieredSentimentAdjustmentStore(
            new Dictionary<string, TieredSentimentAdjustment>(),
            new List<string>());
    }

    public static ITieredSentimentAdjustmentStore NewCachedTieredSentimentAdjustmentStore(MemoryTieredSentimentAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedTieredSentimentAdjustmentStore(
            inner,
            TieredSentimentAdjustmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<TieredSentimentAdjustment> FindTieredSentimentAdjustments(ITieredSentimentAdjustmentStore store, TieredSentimentAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
