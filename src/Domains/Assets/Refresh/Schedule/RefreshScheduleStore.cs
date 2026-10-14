namespace Acme.Workforce.Assets.Refresh.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh schedules by code.</summary>
public interface IRefreshScheduleStore
{
    RefreshSchedule? Find(string code);
    void Save(RefreshSchedule item);
    bool Remove(string code);
    List<RefreshSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshScheduleStore(Dictionary<string, RefreshSchedule> items, List<string> order) : IRefreshScheduleStore
{
    public Dictionary<string, RefreshSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshSchedule item)
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

    public List<RefreshSchedule> List()
    {
        var outValue = new List<RefreshSchedule>();
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
public sealed class CachedRefreshScheduleStore(MemoryRefreshScheduleStore inner, RefreshScheduleCache cache, int hits, int misses) : IRefreshScheduleStore
{
    public MemoryRefreshScheduleStore Inner { get; set; } = inner;
    public RefreshScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshSchedule? Find(string code)
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

    public void Save(RefreshSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh schedules. Empty fields match all.</summary>
public sealed record RefreshScheduleQuery(string Country, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshSchedule item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RefreshSchedule> Apply(List<RefreshSchedule> items)
    {
        var matched = Collections.Filter(items, (RefreshSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh schedule store.</summary>
public static class RefreshScheduleStore
{
    public static IRefreshScheduleStore NewMemoryRefreshScheduleStore()
    {
        return new MemoryRefreshScheduleStore(new Dictionary<string, RefreshSchedule>(), new List<string>());
    }

    public static IRefreshScheduleStore NewCachedRefreshScheduleStore(MemoryRefreshScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshScheduleStore(inner, RefreshScheduleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RefreshSchedule> FindRefreshSchedules(IRefreshScheduleStore store, RefreshScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
