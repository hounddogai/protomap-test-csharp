namespace Acme.Workforce.Absence.Holiday.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday rollups by code.</summary>
public interface IHolidayRollupStore
{
    HolidayRollup? Find(string code);
    void Save(HolidayRollup item);
    bool Remove(string code);
    List<HolidayRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayRollupStore(Dictionary<string, HolidayRollup> items, List<string> order) : IHolidayRollupStore
{
    public Dictionary<string, HolidayRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayRollup item)
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

    public List<HolidayRollup> List()
    {
        var outValue = new List<HolidayRollup>();
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
public sealed class CachedHolidayRollupStore(MemoryHolidayRollupStore inner, HolidayRollupCache cache, int hits, int misses) : IHolidayRollupStore
{
    public MemoryHolidayRollupStore Inner { get; set; } = inner;
    public HolidayRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayRollup? Find(string code)
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

    public void Save(HolidayRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday rollups. Empty fields match all.</summary>
public sealed record HolidayRollupQuery(string Label, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayRollup item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidayRollup> Apply(List<HolidayRollup> items)
    {
        var matched = Collections.Filter(items, (HolidayRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday rollup store.</summary>
public static class HolidayRollupStore
{
    public static IHolidayRollupStore NewMemoryHolidayRollupStore()
    {
        return new MemoryHolidayRollupStore(new Dictionary<string, HolidayRollup>(), new List<string>());
    }

    public static IHolidayRollupStore NewCachedHolidayRollupStore(MemoryHolidayRollupStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayRollupStore(inner, HolidayRollupCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<HolidayRollup> FindHolidayRollups(IHolidayRollupStore store, HolidayRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
