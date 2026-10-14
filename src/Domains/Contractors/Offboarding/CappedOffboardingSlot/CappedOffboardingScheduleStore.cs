namespace Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped offboarding schedules by code.</summary>
public interface ICappedOffboardingScheduleStore
{
    CappedOffboardingSchedule? Find(string code);
    void Save(CappedOffboardingSchedule item);
    bool Remove(string code);
    List<CappedOffboardingSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedOffboardingScheduleStore(Dictionary<string, CappedOffboardingSchedule> items, List<string> order) : ICappedOffboardingScheduleStore
{
    public Dictionary<string, CappedOffboardingSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedOffboardingSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedOffboardingSchedule item)
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

    public List<CappedOffboardingSchedule> List()
    {
        var outValue = new List<CappedOffboardingSchedule>();
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
public sealed class CachedCappedOffboardingScheduleStore(MemoryCappedOffboardingScheduleStore inner, CappedOffboardingScheduleCache cache, int hits, int misses) : ICappedOffboardingScheduleStore
{
    public MemoryCappedOffboardingScheduleStore Inner { get; set; } = inner;
    public CappedOffboardingScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedOffboardingSchedule? Find(string code)
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

    public void Save(CappedOffboardingSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedOffboardingSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped offboarding schedules. Empty fields match all.</summary>
public sealed record CappedOffboardingScheduleQuery(string Locale, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(CappedOffboardingSchedule item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedOffboardingSchedule> Apply(List<CappedOffboardingSchedule> items)
    {
        var matched = Collections.Filter(items, (CappedOffboardingSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped offboarding schedule store.</summary>
public static class CappedOffboardingScheduleStore
{
    public static ICappedOffboardingScheduleStore NewMemoryCappedOffboardingScheduleStore()
    {
        return new MemoryCappedOffboardingScheduleStore(
            new Dictionary<string, CappedOffboardingSchedule>(),
            new List<string>());
    }

    public static ICappedOffboardingScheduleStore NewCachedCappedOffboardingScheduleStore(MemoryCappedOffboardingScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedCappedOffboardingScheduleStore(
            inner,
            CappedOffboardingScheduleCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<CappedOffboardingSchedule> FindCappedOffboardingSchedules(ICappedOffboardingScheduleStore store, CappedOffboardingScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
