namespace Acme.Workforce.Absence.Holiday.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday overrides by code.</summary>
public interface IHolidayOverrideStore
{
    HolidayOverride? Find(string code);
    void Save(HolidayOverride item);
    bool Remove(string code);
    List<HolidayOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayOverrideStore(Dictionary<string, HolidayOverride> items, List<string> order) : IHolidayOverrideStore
{
    public Dictionary<string, HolidayOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayOverride item)
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

    public List<HolidayOverride> List()
    {
        var outValue = new List<HolidayOverride>();
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
public sealed class CachedHolidayOverrideStore(MemoryHolidayOverrideStore inner, HolidayOverrideCache cache, int hits, int misses) : IHolidayOverrideStore
{
    public MemoryHolidayOverrideStore Inner { get; set; } = inner;
    public HolidayOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayOverride? Find(string code)
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

    public void Save(HolidayOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday overrides. Empty fields match all.</summary>
public sealed record HolidayOverrideQuery(string Channel, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayOverride item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidayOverride> Apply(List<HolidayOverride> items)
    {
        var matched = Collections.Filter(items, (HolidayOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday override store.</summary>
public static class HolidayOverrideStore
{
    public static IHolidayOverrideStore NewMemoryHolidayOverrideStore()
    {
        return new MemoryHolidayOverrideStore(new Dictionary<string, HolidayOverride>(), new List<string>());
    }

    public static IHolidayOverrideStore NewCachedHolidayOverrideStore(MemoryHolidayOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayOverrideStore(inner, HolidayOverrideCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<HolidayOverride> FindHolidayOverrides(IHolidayOverrideStore store, HolidayOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
