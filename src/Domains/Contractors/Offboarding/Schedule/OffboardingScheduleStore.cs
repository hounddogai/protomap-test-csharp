namespace Acme.Workforce.Contractors.Offboarding.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding schedules by code.</summary>
public interface IOffboardingScheduleStore
{
    OffboardingSchedule? Find(string code);
    void Save(OffboardingSchedule item);
    bool Remove(string code);
    List<OffboardingSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingScheduleStore(Dictionary<string, OffboardingSchedule> items, List<string> order) : IOffboardingScheduleStore
{
    public Dictionary<string, OffboardingSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingSchedule item)
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

    public List<OffboardingSchedule> List()
    {
        var outValue = new List<OffboardingSchedule>();
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
public sealed class CachedOffboardingScheduleStore(MemoryOffboardingScheduleStore inner, OffboardingScheduleCache cache, int hits, int misses) : IOffboardingScheduleStore
{
    public MemoryOffboardingScheduleStore Inner { get; set; } = inner;
    public OffboardingScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingSchedule? Find(string code)
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

    public void Save(OffboardingSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding schedules. Empty fields match all.</summary>
public sealed record OffboardingScheduleQuery(string Category, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingSchedule item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<OffboardingSchedule> Apply(List<OffboardingSchedule> items)
    {
        var matched = Collections.Filter(items, (OffboardingSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding schedule store.</summary>
public static class OffboardingScheduleStore
{
    public static IOffboardingScheduleStore NewMemoryOffboardingScheduleStore()
    {
        return new MemoryOffboardingScheduleStore(new Dictionary<string, OffboardingSchedule>(), new List<string>());
    }

    public static IOffboardingScheduleStore NewCachedOffboardingScheduleStore(MemoryOffboardingScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingScheduleStore(inner, OffboardingScheduleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<OffboardingSchedule> FindOffboardingSchedules(IOffboardingScheduleStore store, OffboardingScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
