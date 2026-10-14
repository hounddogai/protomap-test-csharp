namespace Acme.Workforce.Compensation.BonusPool.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool schedules by code.</summary>
public interface IBonusPoolScheduleStore
{
    BonusPoolSchedule? Find(string code);
    void Save(BonusPoolSchedule item);
    bool Remove(string code);
    List<BonusPoolSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolScheduleStore(Dictionary<string, BonusPoolSchedule> items, List<string> order) : IBonusPoolScheduleStore
{
    public Dictionary<string, BonusPoolSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolSchedule item)
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

    public List<BonusPoolSchedule> List()
    {
        var outValue = new List<BonusPoolSchedule>();
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
public sealed class CachedBonusPoolScheduleStore(MemoryBonusPoolScheduleStore inner, BonusPoolScheduleCache cache, int hits, int misses) : IBonusPoolScheduleStore
{
    public MemoryBonusPoolScheduleStore Inner { get; set; } = inner;
    public BonusPoolScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolSchedule? Find(string code)
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

    public void Save(BonusPoolSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool schedules. Empty fields match all.</summary>
public sealed record BonusPoolScheduleQuery(string Locale, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolSchedule item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolSchedule> Apply(List<BonusPoolSchedule> items)
    {
        var matched = Collections.Filter(items, (BonusPoolSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool schedule store.</summary>
public static class BonusPoolScheduleStore
{
    public static IBonusPoolScheduleStore NewMemoryBonusPoolScheduleStore()
    {
        return new MemoryBonusPoolScheduleStore(new Dictionary<string, BonusPoolSchedule>(), new List<string>());
    }

    public static IBonusPoolScheduleStore NewCachedBonusPoolScheduleStore(MemoryBonusPoolScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolScheduleStore(inner, BonusPoolScheduleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BonusPoolSchedule> FindBonusPoolSchedules(IBonusPoolScheduleStore store, BonusPoolScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
