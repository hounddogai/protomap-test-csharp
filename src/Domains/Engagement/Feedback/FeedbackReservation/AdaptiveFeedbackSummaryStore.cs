namespace Acme.Workforce.Engagement.Feedback.FeedbackReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive feedback summaries by code.</summary>
public interface IAdaptiveFeedbackSummaryStore
{
    AdaptiveFeedbackSummary? Find(string code);
    void Save(AdaptiveFeedbackSummary item);
    bool Remove(string code);
    List<AdaptiveFeedbackSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveFeedbackSummaryStore(Dictionary<string, AdaptiveFeedbackSummary> items, List<string> order) : IAdaptiveFeedbackSummaryStore
{
    public Dictionary<string, AdaptiveFeedbackSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveFeedbackSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveFeedbackSummary item)
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

    public List<AdaptiveFeedbackSummary> List()
    {
        var outValue = new List<AdaptiveFeedbackSummary>();
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
public sealed class CachedAdaptiveFeedbackSummaryStore(MemoryAdaptiveFeedbackSummaryStore inner, AdaptiveFeedbackSummaryCache cache, int hits, int misses) : IAdaptiveFeedbackSummaryStore
{
    public MemoryAdaptiveFeedbackSummaryStore Inner { get; set; } = inner;
    public AdaptiveFeedbackSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveFeedbackSummary? Find(string code)
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

    public void Save(AdaptiveFeedbackSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveFeedbackSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive feedback summaries. Empty fields match all.</summary>
public sealed record AdaptiveFeedbackSummaryQuery(string Title, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveFeedbackSummary item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveFeedbackSummary> Apply(List<AdaptiveFeedbackSummary> items)
    {
        var matched = Collections.Filter(items, (AdaptiveFeedbackSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive feedback summary store.</summary>
public static class AdaptiveFeedbackSummaryStore
{
    public static IAdaptiveFeedbackSummaryStore NewMemoryAdaptiveFeedbackSummaryStore()
    {
        return new MemoryAdaptiveFeedbackSummaryStore(
            new Dictionary<string, AdaptiveFeedbackSummary>(),
            new List<string>());
    }

    public static IAdaptiveFeedbackSummaryStore NewCachedAdaptiveFeedbackSummaryStore(MemoryAdaptiveFeedbackSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveFeedbackSummaryStore(
            inner,
            AdaptiveFeedbackSummaryCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveFeedbackSummary> FindAdaptiveFeedbackSummaries(IAdaptiveFeedbackSummaryStore store, AdaptiveFeedbackSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
