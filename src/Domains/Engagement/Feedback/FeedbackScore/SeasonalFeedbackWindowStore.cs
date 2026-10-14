namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal feedback windows by code.</summary>
public interface ISeasonalFeedbackWindowStore
{
    SeasonalFeedbackWindow? Find(string code);
    void Save(SeasonalFeedbackWindow item);
    bool Remove(string code);
    List<SeasonalFeedbackWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalFeedbackWindowStore(Dictionary<string, SeasonalFeedbackWindow> items, List<string> order) : ISeasonalFeedbackWindowStore
{
    public Dictionary<string, SeasonalFeedbackWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalFeedbackWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalFeedbackWindow item)
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

    public List<SeasonalFeedbackWindow> List()
    {
        var outValue = new List<SeasonalFeedbackWindow>();
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
public sealed class CachedSeasonalFeedbackWindowStore(MemorySeasonalFeedbackWindowStore inner, SeasonalFeedbackWindowCache cache, int hits, int misses) : ISeasonalFeedbackWindowStore
{
    public MemorySeasonalFeedbackWindowStore Inner { get; set; } = inner;
    public SeasonalFeedbackWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalFeedbackWindow? Find(string code)
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

    public void Save(SeasonalFeedbackWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalFeedbackWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal feedback windows. Empty fields match all.</summary>
public sealed record SeasonalFeedbackWindowQuery(string Country, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalFeedbackWindow item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalFeedbackWindow> Apply(List<SeasonalFeedbackWindow> items)
    {
        var matched = Collections.Filter(items, (SeasonalFeedbackWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal feedback window store.</summary>
public static class SeasonalFeedbackWindowStore
{
    public static ISeasonalFeedbackWindowStore NewMemorySeasonalFeedbackWindowStore()
    {
        return new MemorySeasonalFeedbackWindowStore(
            new Dictionary<string, SeasonalFeedbackWindow>(),
            new List<string>());
    }

    public static ISeasonalFeedbackWindowStore NewCachedSeasonalFeedbackWindowStore(MemorySeasonalFeedbackWindowStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalFeedbackWindowStore(
            inner,
            SeasonalFeedbackWindowCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalFeedbackWindow> FindSeasonalFeedbackWindows(ISeasonalFeedbackWindowStore store, SeasonalFeedbackWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
