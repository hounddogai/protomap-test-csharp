namespace Acme.Workforce.Absence.Holiday.HolidayPolicy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual holiday cycles by code.</summary>
public interface IManualHolidayCycleStore
{
    ManualHolidayCycle? Find(string code);
    void Save(ManualHolidayCycle item);
    bool Remove(string code);
    List<ManualHolidayCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualHolidayCycleStore(Dictionary<string, ManualHolidayCycle> items, List<string> order) : IManualHolidayCycleStore
{
    public Dictionary<string, ManualHolidayCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualHolidayCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualHolidayCycle item)
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

    public List<ManualHolidayCycle> List()
    {
        var outValue = new List<ManualHolidayCycle>();
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
public sealed class CachedManualHolidayCycleStore(MemoryManualHolidayCycleStore inner, ManualHolidayCycleCache cache, int hits, int misses) : IManualHolidayCycleStore
{
    public MemoryManualHolidayCycleStore Inner { get; set; } = inner;
    public ManualHolidayCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualHolidayCycle? Find(string code)
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

    public void Save(ManualHolidayCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualHolidayCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual holiday cycles. Empty fields match all.</summary>
public sealed record ManualHolidayCycleQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ManualHolidayCycle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualHolidayCycle> Apply(List<ManualHolidayCycle> items)
    {
        var matched = Collections.Filter(items, (ManualHolidayCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual holiday cycle store.</summary>
public static class ManualHolidayCycleStore
{
    public static IManualHolidayCycleStore NewMemoryManualHolidayCycleStore()
    {
        return new MemoryManualHolidayCycleStore(new Dictionary<string, ManualHolidayCycle>(), new List<string>());
    }

    public static IManualHolidayCycleStore NewCachedManualHolidayCycleStore(MemoryManualHolidayCycleStore inner, int ttl, IClock clock)
    {
        return new CachedManualHolidayCycleStore(inner, ManualHolidayCycleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ManualHolidayCycle> FindManualHolidayCycles(IManualHolidayCycleStore store, ManualHolidayCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
