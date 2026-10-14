namespace Acme.Workforce.Engagement.Feedback.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive feedback scores by code.</summary>
public interface IAdaptiveFeedbackScoreStore
{
    AdaptiveFeedbackScore? Find(string code);
    void Save(AdaptiveFeedbackScore item);
    bool Remove(string code);
    List<AdaptiveFeedbackScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveFeedbackScoreStore(Dictionary<string, AdaptiveFeedbackScore> items, List<string> order) : IAdaptiveFeedbackScoreStore
{
    public Dictionary<string, AdaptiveFeedbackScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveFeedbackScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveFeedbackScore item)
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

    public List<AdaptiveFeedbackScore> List()
    {
        var outValue = new List<AdaptiveFeedbackScore>();
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
public sealed class CachedAdaptiveFeedbackScoreStore(MemoryAdaptiveFeedbackScoreStore inner, AdaptiveFeedbackScoreCache cache, int hits, int misses) : IAdaptiveFeedbackScoreStore
{
    public MemoryAdaptiveFeedbackScoreStore Inner { get; set; } = inner;
    public AdaptiveFeedbackScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveFeedbackScore? Find(string code)
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

    public void Save(AdaptiveFeedbackScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveFeedbackScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive feedback scores. Empty fields match all.</summary>
public sealed record AdaptiveFeedbackScoreQuery(string Country, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveFeedbackScore item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<AdaptiveFeedbackScore> Apply(List<AdaptiveFeedbackScore> items)
    {
        var matched = Collections.Filter(items, (AdaptiveFeedbackScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive feedback score store.</summary>
public static class AdaptiveFeedbackScoreStore
{
    public static IAdaptiveFeedbackScoreStore NewMemoryAdaptiveFeedbackScoreStore()
    {
        return new MemoryAdaptiveFeedbackScoreStore(
            new Dictionary<string, AdaptiveFeedbackScore>(),
            new List<string>());
    }

    public static IAdaptiveFeedbackScoreStore NewCachedAdaptiveFeedbackScoreStore(MemoryAdaptiveFeedbackScoreStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveFeedbackScoreStore(inner, AdaptiveFeedbackScoreCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AdaptiveFeedbackScore> FindAdaptiveFeedbackScores(IAdaptiveFeedbackScoreStore store, AdaptiveFeedbackScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
