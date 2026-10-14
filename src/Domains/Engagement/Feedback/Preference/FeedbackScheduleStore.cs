namespace Acme.Workforce.Engagement.Feedback.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback schedules by code.</summary>
public interface IFeedbackScheduleStore
{
    FeedbackSchedule? Find(string code);
    void Save(FeedbackSchedule item);
    bool Remove(string code);
    List<FeedbackSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackScheduleStore(Dictionary<string, FeedbackSchedule> items, List<string> order) : IFeedbackScheduleStore
{
    public Dictionary<string, FeedbackSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackSchedule item)
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

    public List<FeedbackSchedule> List()
    {
        var outValue = new List<FeedbackSchedule>();
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
public sealed class CachedFeedbackScheduleStore(MemoryFeedbackScheduleStore inner, FeedbackScheduleCache cache, int hits, int misses) : IFeedbackScheduleStore
{
    public MemoryFeedbackScheduleStore Inner { get; set; } = inner;
    public FeedbackScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackSchedule? Find(string code)
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

    public void Save(FeedbackSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback schedules. Empty fields match all.</summary>
public sealed record FeedbackScheduleQuery(string Label, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackSchedule item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FeedbackSchedule> Apply(List<FeedbackSchedule> items)
    {
        var matched = Collections.Filter(items, (FeedbackSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback schedule store.</summary>
public static class FeedbackScheduleStore
{
    public static IFeedbackScheduleStore NewMemoryFeedbackScheduleStore()
    {
        return new MemoryFeedbackScheduleStore(new Dictionary<string, FeedbackSchedule>(), new List<string>());
    }

    public static IFeedbackScheduleStore NewCachedFeedbackScheduleStore(MemoryFeedbackScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackScheduleStore(inner, FeedbackScheduleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<FeedbackSchedule> FindFeedbackSchedules(IFeedbackScheduleStore store, FeedbackScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
