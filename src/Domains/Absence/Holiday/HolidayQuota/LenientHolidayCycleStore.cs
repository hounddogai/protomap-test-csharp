namespace Acme.Workforce.Absence.Holiday.HolidayQuota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient holiday cycles by code.</summary>
public interface ILenientHolidayCycleStore
{
    LenientHolidayCycle? Find(string code);
    void Save(LenientHolidayCycle item);
    bool Remove(string code);
    List<LenientHolidayCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientHolidayCycleStore(Dictionary<string, LenientHolidayCycle> items, List<string> order) : ILenientHolidayCycleStore
{
    public Dictionary<string, LenientHolidayCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientHolidayCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientHolidayCycle item)
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

    public List<LenientHolidayCycle> List()
    {
        var outValue = new List<LenientHolidayCycle>();
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
public sealed class CachedLenientHolidayCycleStore(MemoryLenientHolidayCycleStore inner, LenientHolidayCycleCache cache, int hits, int misses) : ILenientHolidayCycleStore
{
    public MemoryLenientHolidayCycleStore Inner { get; set; } = inner;
    public LenientHolidayCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientHolidayCycle? Find(string code)
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

    public void Save(LenientHolidayCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientHolidayCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient holiday cycles. Empty fields match all.</summary>
public sealed record LenientHolidayCycleQuery(string Team, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(LenientHolidayCycle item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientHolidayCycle> Apply(List<LenientHolidayCycle> items)
    {
        var matched = Collections.Filter(items, (LenientHolidayCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient holiday cycle store.</summary>
public static class LenientHolidayCycleStore
{
    public static ILenientHolidayCycleStore NewMemoryLenientHolidayCycleStore()
    {
        return new MemoryLenientHolidayCycleStore(new Dictionary<string, LenientHolidayCycle>(), new List<string>());
    }

    public static ILenientHolidayCycleStore NewCachedLenientHolidayCycleStore(MemoryLenientHolidayCycleStore inner, int ttl, IClock clock)
    {
        return new CachedLenientHolidayCycleStore(inner, LenientHolidayCycleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LenientHolidayCycle> FindLenientHolidayCycles(ILenientHolidayCycleStore store, LenientHolidayCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
