namespace Acme.Workforce.Talent.CareerPath.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path schedules by code.</summary>
public interface ICareerPathScheduleStore
{
    CareerPathSchedule? Find(string code);
    void Save(CareerPathSchedule item);
    bool Remove(string code);
    List<CareerPathSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathScheduleStore(Dictionary<string, CareerPathSchedule> items, List<string> order) : ICareerPathScheduleStore
{
    public Dictionary<string, CareerPathSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathSchedule item)
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

    public List<CareerPathSchedule> List()
    {
        var outValue = new List<CareerPathSchedule>();
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
public sealed class CachedCareerPathScheduleStore(MemoryCareerPathScheduleStore inner, CareerPathScheduleCache cache, int hits, int misses) : ICareerPathScheduleStore
{
    public MemoryCareerPathScheduleStore Inner { get; set; } = inner;
    public CareerPathScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathSchedule? Find(string code)
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

    public void Save(CareerPathSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path schedules. Empty fields match all.</summary>
public sealed record CareerPathScheduleQuery(string Segment, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathSchedule item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathSchedule> Apply(List<CareerPathSchedule> items)
    {
        var matched = Collections.Filter(items, (CareerPathSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path schedule store.</summary>
public static class CareerPathScheduleStore
{
    public static ICareerPathScheduleStore NewMemoryCareerPathScheduleStore()
    {
        return new MemoryCareerPathScheduleStore(new Dictionary<string, CareerPathSchedule>(), new List<string>());
    }

    public static ICareerPathScheduleStore NewCachedCareerPathScheduleStore(MemoryCareerPathScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathScheduleStore(inner, CareerPathScheduleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<CareerPathSchedule> FindCareerPathSchedules(ICareerPathScheduleStore store, CareerPathScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
