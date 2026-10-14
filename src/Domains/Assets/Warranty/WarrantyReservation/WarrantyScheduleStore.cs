namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty schedules by code.</summary>
public interface IWarrantyScheduleStore
{
    WarrantySchedule? Find(string code);
    void Save(WarrantySchedule item);
    bool Remove(string code);
    List<WarrantySchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyScheduleStore(Dictionary<string, WarrantySchedule> items, List<string> order) : IWarrantyScheduleStore
{
    public Dictionary<string, WarrantySchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantySchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantySchedule item)
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

    public List<WarrantySchedule> List()
    {
        var outValue = new List<WarrantySchedule>();
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
public sealed class CachedWarrantyScheduleStore(MemoryWarrantyScheduleStore inner, WarrantyScheduleCache cache, int hits, int misses) : IWarrantyScheduleStore
{
    public MemoryWarrantyScheduleStore Inner { get; set; } = inner;
    public WarrantyScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantySchedule? Find(string code)
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

    public void Save(WarrantySchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantySchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty schedules. Empty fields match all.</summary>
public sealed record WarrantyScheduleQuery(string Department, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantySchedule item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantySchedule> Apply(List<WarrantySchedule> items)
    {
        var matched = Collections.Filter(items, (WarrantySchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty schedule store.</summary>
public static class WarrantyScheduleStore
{
    public static IWarrantyScheduleStore NewMemoryWarrantyScheduleStore()
    {
        return new MemoryWarrantyScheduleStore(new Dictionary<string, WarrantySchedule>(), new List<string>());
    }

    public static IWarrantyScheduleStore NewCachedWarrantyScheduleStore(MemoryWarrantyScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyScheduleStore(inner, WarrantyScheduleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<WarrantySchedule> FindWarrantySchedules(IWarrantyScheduleStore store, WarrantyScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
