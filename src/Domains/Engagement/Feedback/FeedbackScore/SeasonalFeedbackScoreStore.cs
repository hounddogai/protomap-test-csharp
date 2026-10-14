namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal feedback scores by code.</summary>
public interface ISeasonalFeedbackScoreStore
{
    SeasonalFeedbackScore? Find(string code);
    void Save(SeasonalFeedbackScore item);
    bool Remove(string code);
    List<SeasonalFeedbackScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalFeedbackScoreStore(Dictionary<string, SeasonalFeedbackScore> items, List<string> order) : ISeasonalFeedbackScoreStore
{
    public Dictionary<string, SeasonalFeedbackScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalFeedbackScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalFeedbackScore item)
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

    public List<SeasonalFeedbackScore> List()
    {
        var outValue = new List<SeasonalFeedbackScore>();
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
public sealed class CachedSeasonalFeedbackScoreStore(MemorySeasonalFeedbackScoreStore inner, SeasonalFeedbackScoreCache cache, int hits, int misses) : ISeasonalFeedbackScoreStore
{
    public MemorySeasonalFeedbackScoreStore Inner { get; set; } = inner;
    public SeasonalFeedbackScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalFeedbackScore? Find(string code)
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

    public void Save(SeasonalFeedbackScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalFeedbackScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal feedback scores. Empty fields match all.</summary>
public sealed record SeasonalFeedbackScoreQuery(string Country, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalFeedbackScore item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalFeedbackScore> Apply(List<SeasonalFeedbackScore> items)
    {
        var matched = Collections.Filter(items, (SeasonalFeedbackScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal feedback score store.</summary>
public static class SeasonalFeedbackScoreStore
{
    public static ISeasonalFeedbackScoreStore NewMemorySeasonalFeedbackScoreStore()
    {
        return new MemorySeasonalFeedbackScoreStore(
            new Dictionary<string, SeasonalFeedbackScore>(),
            new List<string>());
    }

    public static ISeasonalFeedbackScoreStore NewCachedSeasonalFeedbackScoreStore(MemorySeasonalFeedbackScoreStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalFeedbackScoreStore(inner, SeasonalFeedbackScoreCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SeasonalFeedbackScore> FindSeasonalFeedbackScores(ISeasonalFeedbackScoreStore store, SeasonalFeedbackScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
