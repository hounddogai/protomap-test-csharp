namespace Acme.Workforce.Engagement.Sentiment.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual sentiment adjustments by code.</summary>
public interface IManualSentimentAdjustmentStore
{
    ManualSentimentAdjustment? Find(string code);
    void Save(ManualSentimentAdjustment item);
    bool Remove(string code);
    List<ManualSentimentAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualSentimentAdjustmentStore(Dictionary<string, ManualSentimentAdjustment> items, List<string> order) : IManualSentimentAdjustmentStore
{
    public Dictionary<string, ManualSentimentAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualSentimentAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualSentimentAdjustment item)
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

    public List<ManualSentimentAdjustment> List()
    {
        var outValue = new List<ManualSentimentAdjustment>();
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
public sealed class CachedManualSentimentAdjustmentStore(MemoryManualSentimentAdjustmentStore inner, ManualSentimentAdjustmentCache cache, int hits, int misses) : IManualSentimentAdjustmentStore
{
    public MemoryManualSentimentAdjustmentStore Inner { get; set; } = inner;
    public ManualSentimentAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualSentimentAdjustment? Find(string code)
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

    public void Save(ManualSentimentAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualSentimentAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual sentiment adjustments. Empty fields match all.</summary>
public sealed record ManualSentimentAdjustmentQuery(string Name, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(ManualSentimentAdjustment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualSentimentAdjustment> Apply(List<ManualSentimentAdjustment> items)
    {
        var matched = Collections.Filter(items, (ManualSentimentAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual sentiment adjustment store.</summary>
public static class ManualSentimentAdjustmentStore
{
    public static IManualSentimentAdjustmentStore NewMemoryManualSentimentAdjustmentStore()
    {
        return new MemoryManualSentimentAdjustmentStore(
            new Dictionary<string, ManualSentimentAdjustment>(),
            new List<string>());
    }

    public static IManualSentimentAdjustmentStore NewCachedManualSentimentAdjustmentStore(MemoryManualSentimentAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedManualSentimentAdjustmentStore(
            inner,
            ManualSentimentAdjustmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ManualSentimentAdjustment> FindManualSentimentAdjustments(IManualSentimentAdjustmentStore store, ManualSentimentAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
