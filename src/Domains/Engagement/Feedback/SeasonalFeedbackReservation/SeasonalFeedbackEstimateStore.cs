namespace Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal feedback estimates by code.</summary>
public interface ISeasonalFeedbackEstimateStore
{
    SeasonalFeedbackEstimate? Find(string code);
    void Save(SeasonalFeedbackEstimate item);
    bool Remove(string code);
    List<SeasonalFeedbackEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalFeedbackEstimateStore(Dictionary<string, SeasonalFeedbackEstimate> items, List<string> order) : ISeasonalFeedbackEstimateStore
{
    public Dictionary<string, SeasonalFeedbackEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalFeedbackEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalFeedbackEstimate item)
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

    public List<SeasonalFeedbackEstimate> List()
    {
        var outValue = new List<SeasonalFeedbackEstimate>();
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
public sealed class CachedSeasonalFeedbackEstimateStore(MemorySeasonalFeedbackEstimateStore inner, SeasonalFeedbackEstimateCache cache, int hits, int misses) : ISeasonalFeedbackEstimateStore
{
    public MemorySeasonalFeedbackEstimateStore Inner { get; set; } = inner;
    public SeasonalFeedbackEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalFeedbackEstimate? Find(string code)
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

    public void Save(SeasonalFeedbackEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalFeedbackEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal feedback estimates. Empty fields match all.</summary>
public sealed record SeasonalFeedbackEstimateQuery(string Label, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalFeedbackEstimate item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalFeedbackEstimate> Apply(List<SeasonalFeedbackEstimate> items)
    {
        var matched = Collections.Filter(items, (SeasonalFeedbackEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal feedback estimate store.</summary>
public static class SeasonalFeedbackEstimateStore
{
    public static ISeasonalFeedbackEstimateStore NewMemorySeasonalFeedbackEstimateStore()
    {
        return new MemorySeasonalFeedbackEstimateStore(
            new Dictionary<string, SeasonalFeedbackEstimate>(),
            new List<string>());
    }

    public static ISeasonalFeedbackEstimateStore NewCachedSeasonalFeedbackEstimateStore(MemorySeasonalFeedbackEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalFeedbackEstimateStore(
            inner,
            SeasonalFeedbackEstimateCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalFeedbackEstimate> FindSeasonalFeedbackEstimates(ISeasonalFeedbackEstimateStore store, SeasonalFeedbackEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
