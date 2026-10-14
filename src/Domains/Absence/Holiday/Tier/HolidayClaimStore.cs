namespace Acme.Workforce.Absence.Holiday.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday claims by code.</summary>
public interface IHolidayClaimStore
{
    HolidayClaim? Find(string code);
    void Save(HolidayClaim item);
    bool Remove(string code);
    List<HolidayClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayClaimStore(Dictionary<string, HolidayClaim> items, List<string> order) : IHolidayClaimStore
{
    public Dictionary<string, HolidayClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayClaim item)
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

    public List<HolidayClaim> List()
    {
        var outValue = new List<HolidayClaim>();
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
public sealed class CachedHolidayClaimStore(MemoryHolidayClaimStore inner, HolidayClaimCache cache, int hits, int misses) : IHolidayClaimStore
{
    public MemoryHolidayClaimStore Inner { get; set; } = inner;
    public HolidayClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayClaim? Find(string code)
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

    public void Save(HolidayClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday claims. Empty fields match all.</summary>
public sealed record HolidayClaimQuery(string Name, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayClaim item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidayClaim> Apply(List<HolidayClaim> items)
    {
        var matched = Collections.Filter(items, (HolidayClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday claim store.</summary>
public static class HolidayClaimStore
{
    public static IHolidayClaimStore NewMemoryHolidayClaimStore()
    {
        return new MemoryHolidayClaimStore(new Dictionary<string, HolidayClaim>(), new List<string>());
    }

    public static IHolidayClaimStore NewCachedHolidayClaimStore(MemoryHolidayClaimStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayClaimStore(inner, HolidayClaimCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<HolidayClaim> FindHolidayClaims(IHolidayClaimStore store, HolidayClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
