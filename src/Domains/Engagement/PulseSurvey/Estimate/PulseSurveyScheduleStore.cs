namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey schedules by code.</summary>
public interface IPulseSurveyScheduleStore
{
    PulseSurveySchedule? Find(string code);
    void Save(PulseSurveySchedule item);
    bool Remove(string code);
    List<PulseSurveySchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyScheduleStore(Dictionary<string, PulseSurveySchedule> items, List<string> order) : IPulseSurveyScheduleStore
{
    public Dictionary<string, PulseSurveySchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveySchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveySchedule item)
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

    public List<PulseSurveySchedule> List()
    {
        var outValue = new List<PulseSurveySchedule>();
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
public sealed class CachedPulseSurveyScheduleStore(MemoryPulseSurveyScheduleStore inner, PulseSurveyScheduleCache cache, int hits, int misses) : IPulseSurveyScheduleStore
{
    public MemoryPulseSurveyScheduleStore Inner { get; set; } = inner;
    public PulseSurveyScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveySchedule? Find(string code)
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

    public void Save(PulseSurveySchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveySchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey schedules. Empty fields match all.</summary>
public sealed record PulseSurveyScheduleQuery(string Title, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveySchedule item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<PulseSurveySchedule> Apply(List<PulseSurveySchedule> items)
    {
        var matched = Collections.Filter(items, (PulseSurveySchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey schedule store.</summary>
public static class PulseSurveyScheduleStore
{
    public static IPulseSurveyScheduleStore NewMemoryPulseSurveyScheduleStore()
    {
        return new MemoryPulseSurveyScheduleStore(new Dictionary<string, PulseSurveySchedule>(), new List<string>());
    }

    public static IPulseSurveyScheduleStore NewCachedPulseSurveyScheduleStore(MemoryPulseSurveyScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyScheduleStore(inner, PulseSurveyScheduleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PulseSurveySchedule> FindPulseSurveySchedules(IPulseSurveyScheduleStore store, PulseSurveyScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
