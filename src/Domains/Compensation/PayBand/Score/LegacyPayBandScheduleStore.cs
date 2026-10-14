namespace Acme.Workforce.Compensation.PayBand.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy pay band schedules by code.</summary>
public interface ILegacyPayBandScheduleStore
{
    LegacyPayBandSchedule? Find(string code);
    void Save(LegacyPayBandSchedule item);
    bool Remove(string code);
    List<LegacyPayBandSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyPayBandScheduleStore(Dictionary<string, LegacyPayBandSchedule> items, List<string> order) : ILegacyPayBandScheduleStore
{
    public Dictionary<string, LegacyPayBandSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyPayBandSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyPayBandSchedule item)
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

    public List<LegacyPayBandSchedule> List()
    {
        var outValue = new List<LegacyPayBandSchedule>();
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
public sealed class CachedLegacyPayBandScheduleStore(MemoryLegacyPayBandScheduleStore inner, LegacyPayBandScheduleCache cache, int hits, int misses) : ILegacyPayBandScheduleStore
{
    public MemoryLegacyPayBandScheduleStore Inner { get; set; } = inner;
    public LegacyPayBandScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyPayBandSchedule? Find(string code)
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

    public void Save(LegacyPayBandSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyPayBandSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy pay band schedules. Empty fields match all.</summary>
public sealed record LegacyPayBandScheduleQuery(string Country, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyPayBandSchedule item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyPayBandSchedule> Apply(List<LegacyPayBandSchedule> items)
    {
        var matched = Collections.Filter(items, (LegacyPayBandSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy pay band schedule store.</summary>
public static class LegacyPayBandScheduleStore
{
    public static ILegacyPayBandScheduleStore NewMemoryLegacyPayBandScheduleStore()
    {
        return new MemoryLegacyPayBandScheduleStore(
            new Dictionary<string, LegacyPayBandSchedule>(),
            new List<string>());
    }

    public static ILegacyPayBandScheduleStore NewCachedLegacyPayBandScheduleStore(MemoryLegacyPayBandScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyPayBandScheduleStore(inner, LegacyPayBandScheduleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LegacyPayBandSchedule> FindLegacyPayBandSchedules(ILegacyPayBandScheduleStore store, LegacyPayBandScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
