namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday groups by code.</summary>
public interface IHolidayGroupStore
{
    HolidayGroup? Find(string code);
    void Save(HolidayGroup item);
    bool Remove(string code);
    List<HolidayGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayGroupStore(Dictionary<string, HolidayGroup> items, List<string> order) : IHolidayGroupStore
{
    public Dictionary<string, HolidayGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayGroup item)
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

    public List<HolidayGroup> List()
    {
        var outValue = new List<HolidayGroup>();
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
public sealed class CachedHolidayGroupStore(MemoryHolidayGroupStore inner, HolidayGroupCache cache, int hits, int misses) : IHolidayGroupStore
{
    public MemoryHolidayGroupStore Inner { get; set; } = inner;
    public HolidayGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayGroup? Find(string code)
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

    public void Save(HolidayGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday groups. Empty fields match all.</summary>
public sealed record HolidayGroupQuery(string Segment, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayGroup item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidayGroup> Apply(List<HolidayGroup> items)
    {
        var matched = Collections.Filter(items, (HolidayGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday group store.</summary>
public static class HolidayGroupStore
{
    public static IHolidayGroupStore NewMemoryHolidayGroupStore()
    {
        return new MemoryHolidayGroupStore(new Dictionary<string, HolidayGroup>(), new List<string>());
    }

    public static IHolidayGroupStore NewCachedHolidayGroupStore(MemoryHolidayGroupStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayGroupStore(inner, HolidayGroupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<HolidayGroup> FindHolidayGroups(IHolidayGroupStore store, HolidayGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
