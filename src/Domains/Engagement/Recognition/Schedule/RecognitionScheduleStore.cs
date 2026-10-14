namespace Acme.Workforce.Engagement.Recognition.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition schedules by code.</summary>
public interface IRecognitionScheduleStore
{
    RecognitionSchedule? Find(string code);
    void Save(RecognitionSchedule item);
    bool Remove(string code);
    List<RecognitionSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionScheduleStore(Dictionary<string, RecognitionSchedule> items, List<string> order) : IRecognitionScheduleStore
{
    public Dictionary<string, RecognitionSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionSchedule item)
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

    public List<RecognitionSchedule> List()
    {
        var outValue = new List<RecognitionSchedule>();
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
public sealed class CachedRecognitionScheduleStore(MemoryRecognitionScheduleStore inner, RecognitionScheduleCache cache, int hits, int misses) : IRecognitionScheduleStore
{
    public MemoryRecognitionScheduleStore Inner { get; set; } = inner;
    public RecognitionScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionSchedule? Find(string code)
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

    public void Save(RecognitionSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition schedules. Empty fields match all.</summary>
public sealed record RecognitionScheduleQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionSchedule item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RecognitionSchedule> Apply(List<RecognitionSchedule> items)
    {
        var matched = Collections.Filter(items, (RecognitionSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition schedule store.</summary>
public static class RecognitionScheduleStore
{
    public static IRecognitionScheduleStore NewMemoryRecognitionScheduleStore()
    {
        return new MemoryRecognitionScheduleStore(new Dictionary<string, RecognitionSchedule>(), new List<string>());
    }

    public static IRecognitionScheduleStore NewCachedRecognitionScheduleStore(MemoryRecognitionScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionScheduleStore(inner, RecognitionScheduleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RecognitionSchedule> FindRecognitionSchedules(IRecognitionScheduleStore store, RecognitionScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
