namespace Acme.Workforce.Engagement.Sentiment.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment schedules by code.</summary>
public interface ISentimentScheduleStore
{
    SentimentSchedule? Find(string code);
    void Save(SentimentSchedule item);
    bool Remove(string code);
    List<SentimentSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentScheduleStore(Dictionary<string, SentimentSchedule> items, List<string> order) : ISentimentScheduleStore
{
    public Dictionary<string, SentimentSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentSchedule item)
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

    public List<SentimentSchedule> List()
    {
        var outValue = new List<SentimentSchedule>();
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
public sealed class CachedSentimentScheduleStore(MemorySentimentScheduleStore inner, SentimentScheduleCache cache, int hits, int misses) : ISentimentScheduleStore
{
    public MemorySentimentScheduleStore Inner { get; set; } = inner;
    public SentimentScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentSchedule? Find(string code)
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

    public void Save(SentimentSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment schedules. Empty fields match all.</summary>
public sealed record SentimentScheduleQuery(string Name, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentSchedule item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<SentimentSchedule> Apply(List<SentimentSchedule> items)
    {
        var matched = Collections.Filter(items, (SentimentSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment schedule store.</summary>
public static class SentimentScheduleStore
{
    public static ISentimentScheduleStore NewMemorySentimentScheduleStore()
    {
        return new MemorySentimentScheduleStore(new Dictionary<string, SentimentSchedule>(), new List<string>());
    }

    public static ISentimentScheduleStore NewCachedSentimentScheduleStore(MemorySentimentScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentScheduleStore(inner, SentimentScheduleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<SentimentSchedule> FindSentimentSchedules(ISentimentScheduleStore store, SentimentScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
