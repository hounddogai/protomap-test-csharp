namespace Acme.Workforce.Compensation.BonusPool.PoolSchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal bonus pool schedules by code.</summary>
public interface ISeasonalBonusPoolScheduleStore
{
    SeasonalBonusPoolSchedule? Find(string code);
    void Save(SeasonalBonusPoolSchedule item);
    bool Remove(string code);
    List<SeasonalBonusPoolSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalBonusPoolScheduleStore(Dictionary<string, SeasonalBonusPoolSchedule> items, List<string> order) : ISeasonalBonusPoolScheduleStore
{
    public Dictionary<string, SeasonalBonusPoolSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalBonusPoolSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalBonusPoolSchedule item)
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

    public List<SeasonalBonusPoolSchedule> List()
    {
        var outValue = new List<SeasonalBonusPoolSchedule>();
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
public sealed class CachedSeasonalBonusPoolScheduleStore(MemorySeasonalBonusPoolScheduleStore inner, SeasonalBonusPoolScheduleCache cache, int hits, int misses) : ISeasonalBonusPoolScheduleStore
{
    public MemorySeasonalBonusPoolScheduleStore Inner { get; set; } = inner;
    public SeasonalBonusPoolScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalBonusPoolSchedule? Find(string code)
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

    public void Save(SeasonalBonusPoolSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalBonusPoolSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal bonus pool schedules. Empty fields match all.</summary>
public sealed record SeasonalBonusPoolScheduleQuery(string Unit, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalBonusPoolSchedule item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<SeasonalBonusPoolSchedule> Apply(List<SeasonalBonusPoolSchedule> items)
    {
        var matched = Collections.Filter(items, (SeasonalBonusPoolSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal bonus pool schedule store.</summary>
public static class SeasonalBonusPoolScheduleStore
{
    public static ISeasonalBonusPoolScheduleStore NewMemorySeasonalBonusPoolScheduleStore()
    {
        return new MemorySeasonalBonusPoolScheduleStore(
            new Dictionary<string, SeasonalBonusPoolSchedule>(),
            new List<string>());
    }

    public static ISeasonalBonusPoolScheduleStore NewCachedSeasonalBonusPoolScheduleStore(MemorySeasonalBonusPoolScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalBonusPoolScheduleStore(
            inner,
            SeasonalBonusPoolScheduleCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalBonusPoolSchedule> FindSeasonalBonusPoolSchedules(ISeasonalBonusPoolScheduleStore store, SeasonalBonusPoolScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
