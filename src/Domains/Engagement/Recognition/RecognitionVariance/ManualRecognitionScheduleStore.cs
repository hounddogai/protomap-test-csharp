namespace Acme.Workforce.Engagement.Recognition.RecognitionVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual recognition schedules by code.</summary>
public interface IManualRecognitionScheduleStore
{
    ManualRecognitionSchedule? Find(string code);
    void Save(ManualRecognitionSchedule item);
    bool Remove(string code);
    List<ManualRecognitionSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualRecognitionScheduleStore(Dictionary<string, ManualRecognitionSchedule> items, List<string> order) : IManualRecognitionScheduleStore
{
    public Dictionary<string, ManualRecognitionSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualRecognitionSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualRecognitionSchedule item)
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

    public List<ManualRecognitionSchedule> List()
    {
        var outValue = new List<ManualRecognitionSchedule>();
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
public sealed class CachedManualRecognitionScheduleStore(MemoryManualRecognitionScheduleStore inner, ManualRecognitionScheduleCache cache, int hits, int misses) : IManualRecognitionScheduleStore
{
    public MemoryManualRecognitionScheduleStore Inner { get; set; } = inner;
    public ManualRecognitionScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualRecognitionSchedule? Find(string code)
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

    public void Save(ManualRecognitionSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualRecognitionSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual recognition schedules. Empty fields match all.</summary>
public sealed record ManualRecognitionScheduleQuery(string Title, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(ManualRecognitionSchedule item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualRecognitionSchedule> Apply(List<ManualRecognitionSchedule> items)
    {
        var matched = Collections.Filter(items, (ManualRecognitionSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual recognition schedule store.</summary>
public static class ManualRecognitionScheduleStore
{
    public static IManualRecognitionScheduleStore NewMemoryManualRecognitionScheduleStore()
    {
        return new MemoryManualRecognitionScheduleStore(
            new Dictionary<string, ManualRecognitionSchedule>(),
            new List<string>());
    }

    public static IManualRecognitionScheduleStore NewCachedManualRecognitionScheduleStore(MemoryManualRecognitionScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedManualRecognitionScheduleStore(
            inner,
            ManualRecognitionScheduleCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<ManualRecognitionSchedule> FindManualRecognitionSchedules(IManualRecognitionScheduleStore store, ManualRecognitionScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
