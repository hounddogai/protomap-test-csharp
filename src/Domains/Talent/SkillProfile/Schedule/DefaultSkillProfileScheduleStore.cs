namespace Acme.Workforce.Talent.SkillProfile.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default skill profile schedules by code.</summary>
public interface IDefaultSkillProfileScheduleStore
{
    DefaultSkillProfileSchedule? Find(string code);
    void Save(DefaultSkillProfileSchedule item);
    bool Remove(string code);
    List<DefaultSkillProfileSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultSkillProfileScheduleStore(Dictionary<string, DefaultSkillProfileSchedule> items, List<string> order) : IDefaultSkillProfileScheduleStore
{
    public Dictionary<string, DefaultSkillProfileSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultSkillProfileSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultSkillProfileSchedule item)
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

    public List<DefaultSkillProfileSchedule> List()
    {
        var outValue = new List<DefaultSkillProfileSchedule>();
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
public sealed class CachedDefaultSkillProfileScheduleStore(MemoryDefaultSkillProfileScheduleStore inner, DefaultSkillProfileScheduleCache cache, int hits, int misses) : IDefaultSkillProfileScheduleStore
{
    public MemoryDefaultSkillProfileScheduleStore Inner { get; set; } = inner;
    public DefaultSkillProfileScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultSkillProfileSchedule? Find(string code)
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

    public void Save(DefaultSkillProfileSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultSkillProfileSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default skill profile schedules. Empty fields match all.</summary>
public sealed record DefaultSkillProfileScheduleQuery(string Country, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultSkillProfileSchedule item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<DefaultSkillProfileSchedule> Apply(List<DefaultSkillProfileSchedule> items)
    {
        var matched = Collections.Filter(items, (DefaultSkillProfileSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default skill profile schedule store.</summary>
public static class DefaultSkillProfileScheduleStore
{
    public static IDefaultSkillProfileScheduleStore NewMemoryDefaultSkillProfileScheduleStore()
    {
        return new MemoryDefaultSkillProfileScheduleStore(
            new Dictionary<string, DefaultSkillProfileSchedule>(),
            new List<string>());
    }

    public static IDefaultSkillProfileScheduleStore NewCachedDefaultSkillProfileScheduleStore(MemoryDefaultSkillProfileScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultSkillProfileScheduleStore(
            inner,
            DefaultSkillProfileScheduleCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<DefaultSkillProfileSchedule> FindDefaultSkillProfileSchedules(IDefaultSkillProfileScheduleStore store, DefaultSkillProfileScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
