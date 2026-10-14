namespace Acme.Workforce.Orgdesign.Span.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span schedules by code.</summary>
public interface ISpanScheduleStore
{
    SpanSchedule? Find(string code);
    void Save(SpanSchedule item);
    bool Remove(string code);
    List<SpanSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanScheduleStore(Dictionary<string, SpanSchedule> items, List<string> order) : ISpanScheduleStore
{
    public Dictionary<string, SpanSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanSchedule item)
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

    public List<SpanSchedule> List()
    {
        var outValue = new List<SpanSchedule>();
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
public sealed class CachedSpanScheduleStore(MemorySpanScheduleStore inner, SpanScheduleCache cache, int hits, int misses) : ISpanScheduleStore
{
    public MemorySpanScheduleStore Inner { get; set; } = inner;
    public SpanScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanSchedule? Find(string code)
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

    public void Save(SpanSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span schedules. Empty fields match all.</summary>
public sealed record SpanScheduleQuery(string Department, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(SpanSchedule item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<SpanSchedule> Apply(List<SpanSchedule> items)
    {
        var matched = Collections.Filter(items, (SpanSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span schedule store.</summary>
public static class SpanScheduleStore
{
    public static ISpanScheduleStore NewMemorySpanScheduleStore()
    {
        return new MemorySpanScheduleStore(new Dictionary<string, SpanSchedule>(), new List<string>());
    }

    public static ISpanScheduleStore NewCachedSpanScheduleStore(MemorySpanScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedSpanScheduleStore(inner, SpanScheduleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<SpanSchedule> FindSpanSchedules(ISpanScheduleStore store, SpanScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
