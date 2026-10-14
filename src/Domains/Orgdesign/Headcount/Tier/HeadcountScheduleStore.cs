namespace Acme.Workforce.Orgdesign.Headcount.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount schedules by code.</summary>
public interface IHeadcountScheduleStore
{
    HeadcountSchedule? Find(string code);
    void Save(HeadcountSchedule item);
    bool Remove(string code);
    List<HeadcountSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountScheduleStore(Dictionary<string, HeadcountSchedule> items, List<string> order) : IHeadcountScheduleStore
{
    public Dictionary<string, HeadcountSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountSchedule item)
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

    public List<HeadcountSchedule> List()
    {
        var outValue = new List<HeadcountSchedule>();
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
public sealed class CachedHeadcountScheduleStore(MemoryHeadcountScheduleStore inner, HeadcountScheduleCache cache, int hits, int misses) : IHeadcountScheduleStore
{
    public MemoryHeadcountScheduleStore Inner { get; set; } = inner;
    public HeadcountScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountSchedule? Find(string code)
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

    public void Save(HeadcountSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount schedules. Empty fields match all.</summary>
public sealed record HeadcountScheduleQuery(string Unit, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountSchedule item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountSchedule> Apply(List<HeadcountSchedule> items)
    {
        var matched = Collections.Filter(items, (HeadcountSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount schedule store.</summary>
public static class HeadcountScheduleStore
{
    public static IHeadcountScheduleStore NewMemoryHeadcountScheduleStore()
    {
        return new MemoryHeadcountScheduleStore(new Dictionary<string, HeadcountSchedule>(), new List<string>());
    }

    public static IHeadcountScheduleStore NewCachedHeadcountScheduleStore(MemoryHeadcountScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountScheduleStore(inner, HeadcountScheduleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<HeadcountSchedule> FindHeadcountSchedules(IHeadcountScheduleStore store, HeadcountScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
