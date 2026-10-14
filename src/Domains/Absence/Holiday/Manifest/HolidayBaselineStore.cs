namespace Acme.Workforce.Absence.Holiday.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday baselines by code.</summary>
public interface IHolidayBaselineStore
{
    HolidayBaseline? Find(string code);
    void Save(HolidayBaseline item);
    bool Remove(string code);
    List<HolidayBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayBaselineStore(Dictionary<string, HolidayBaseline> items, List<string> order) : IHolidayBaselineStore
{
    public Dictionary<string, HolidayBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayBaseline item)
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

    public List<HolidayBaseline> List()
    {
        var outValue = new List<HolidayBaseline>();
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
public sealed class CachedHolidayBaselineStore(MemoryHolidayBaselineStore inner, HolidayBaselineCache cache, int hits, int misses) : IHolidayBaselineStore
{
    public MemoryHolidayBaselineStore Inner { get; set; } = inner;
    public HolidayBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayBaseline? Find(string code)
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

    public void Save(HolidayBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday baselines. Empty fields match all.</summary>
public sealed record HolidayBaselineQuery(string Team, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayBaseline item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<HolidayBaseline> Apply(List<HolidayBaseline> items)
    {
        var matched = Collections.Filter(items, (HolidayBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday baseline store.</summary>
public static class HolidayBaselineStore
{
    public static IHolidayBaselineStore NewMemoryHolidayBaselineStore()
    {
        return new MemoryHolidayBaselineStore(new Dictionary<string, HolidayBaseline>(), new List<string>());
    }

    public static IHolidayBaselineStore NewCachedHolidayBaselineStore(MemoryHolidayBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayBaselineStore(inner, HolidayBaselineCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<HolidayBaseline> FindHolidayBaselines(IHolidayBaselineStore store, HolidayBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
