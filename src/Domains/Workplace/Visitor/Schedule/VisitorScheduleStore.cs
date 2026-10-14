namespace Acme.Workforce.Workplace.Visitor.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor schedules by code.</summary>
public interface IVisitorScheduleStore
{
    VisitorSchedule? Find(string code);
    void Save(VisitorSchedule item);
    bool Remove(string code);
    List<VisitorSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorScheduleStore(Dictionary<string, VisitorSchedule> items, List<string> order) : IVisitorScheduleStore
{
    public Dictionary<string, VisitorSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorSchedule item)
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

    public List<VisitorSchedule> List()
    {
        var outValue = new List<VisitorSchedule>();
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
public sealed class CachedVisitorScheduleStore(MemoryVisitorScheduleStore inner, VisitorScheduleCache cache, int hits, int misses) : IVisitorScheduleStore
{
    public MemoryVisitorScheduleStore Inner { get; set; } = inner;
    public VisitorScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorSchedule? Find(string code)
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

    public void Save(VisitorSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor schedules. Empty fields match all.</summary>
public sealed record VisitorScheduleQuery(string Segment, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorSchedule item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorSchedule> Apply(List<VisitorSchedule> items)
    {
        var matched = Collections.Filter(items, (VisitorSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor schedule store.</summary>
public static class VisitorScheduleStore
{
    public static IVisitorScheduleStore NewMemoryVisitorScheduleStore()
    {
        return new MemoryVisitorScheduleStore(new Dictionary<string, VisitorSchedule>(), new List<string>());
    }

    public static IVisitorScheduleStore NewCachedVisitorScheduleStore(MemoryVisitorScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorScheduleStore(inner, VisitorScheduleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<VisitorSchedule> FindVisitorSchedules(IVisitorScheduleStore store, VisitorScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
