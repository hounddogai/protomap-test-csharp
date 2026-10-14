namespace Acme.Workforce.Absence.Holiday.HolidayAllocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday profiles by code.</summary>
public interface IHolidayProfileStore
{
    HolidayProfile? Find(string code);
    void Save(HolidayProfile item);
    bool Remove(string code);
    List<HolidayProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayProfileStore(Dictionary<string, HolidayProfile> items, List<string> order) : IHolidayProfileStore
{
    public Dictionary<string, HolidayProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayProfile item)
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

    public List<HolidayProfile> List()
    {
        var outValue = new List<HolidayProfile>();
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
public sealed class CachedHolidayProfileStore(MemoryHolidayProfileStore inner, HolidayProfileCache cache, int hits, int misses) : IHolidayProfileStore
{
    public MemoryHolidayProfileStore Inner { get; set; } = inner;
    public HolidayProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayProfile? Find(string code)
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

    public void Save(HolidayProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday profiles. Empty fields match all.</summary>
public sealed record HolidayProfileQuery(string Source, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayProfile item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidayProfile> Apply(List<HolidayProfile> items)
    {
        var matched = Collections.Filter(items, (HolidayProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday profile store.</summary>
public static class HolidayProfileStore
{
    public static IHolidayProfileStore NewMemoryHolidayProfileStore()
    {
        return new MemoryHolidayProfileStore(new Dictionary<string, HolidayProfile>(), new List<string>());
    }

    public static IHolidayProfileStore NewCachedHolidayProfileStore(MemoryHolidayProfileStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayProfileStore(inner, HolidayProfileCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<HolidayProfile> FindHolidayProfiles(IHolidayProfileStore store, HolidayProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
