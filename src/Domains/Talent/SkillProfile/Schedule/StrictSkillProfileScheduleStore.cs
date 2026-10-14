namespace Acme.Workforce.Talent.SkillProfile.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict skill profile schedules by code.</summary>
public interface IStrictSkillProfileScheduleStore
{
    StrictSkillProfileSchedule? Find(string code);
    void Save(StrictSkillProfileSchedule item);
    bool Remove(string code);
    List<StrictSkillProfileSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictSkillProfileScheduleStore(Dictionary<string, StrictSkillProfileSchedule> items, List<string> order) : IStrictSkillProfileScheduleStore
{
    public Dictionary<string, StrictSkillProfileSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictSkillProfileSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictSkillProfileSchedule item)
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

    public List<StrictSkillProfileSchedule> List()
    {
        var outValue = new List<StrictSkillProfileSchedule>();
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
public sealed class CachedStrictSkillProfileScheduleStore(MemoryStrictSkillProfileScheduleStore inner, StrictSkillProfileScheduleCache cache, int hits, int misses) : IStrictSkillProfileScheduleStore
{
    public MemoryStrictSkillProfileScheduleStore Inner { get; set; } = inner;
    public StrictSkillProfileScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictSkillProfileSchedule? Find(string code)
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

    public void Save(StrictSkillProfileSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictSkillProfileSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict skill profile schedules. Empty fields match all.</summary>
public sealed record StrictSkillProfileScheduleQuery(string Name, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(StrictSkillProfileSchedule item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictSkillProfileSchedule> Apply(List<StrictSkillProfileSchedule> items)
    {
        var matched = Collections.Filter(items, (StrictSkillProfileSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict skill profile schedule store.</summary>
public static class StrictSkillProfileScheduleStore
{
    public static IStrictSkillProfileScheduleStore NewMemoryStrictSkillProfileScheduleStore()
    {
        return new MemoryStrictSkillProfileScheduleStore(
            new Dictionary<string, StrictSkillProfileSchedule>(),
            new List<string>());
    }

    public static IStrictSkillProfileScheduleStore NewCachedStrictSkillProfileScheduleStore(MemoryStrictSkillProfileScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedStrictSkillProfileScheduleStore(
            inner,
            StrictSkillProfileScheduleCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<StrictSkillProfileSchedule> FindStrictSkillProfileSchedules(IStrictSkillProfileScheduleStore store, StrictSkillProfileScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
