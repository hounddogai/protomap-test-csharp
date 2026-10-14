namespace Acme.Workforce.Absence.Holiday.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday milestones by code.</summary>
public interface IHolidayMilestoneStore
{
    HolidayMilestone? Find(string code);
    void Save(HolidayMilestone item);
    bool Remove(string code);
    List<HolidayMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayMilestoneStore(Dictionary<string, HolidayMilestone> items, List<string> order) : IHolidayMilestoneStore
{
    public Dictionary<string, HolidayMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayMilestone item)
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

    public List<HolidayMilestone> List()
    {
        var outValue = new List<HolidayMilestone>();
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
public sealed class CachedHolidayMilestoneStore(MemoryHolidayMilestoneStore inner, HolidayMilestoneCache cache, int hits, int misses) : IHolidayMilestoneStore
{
    public MemoryHolidayMilestoneStore Inner { get; set; } = inner;
    public HolidayMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayMilestone? Find(string code)
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

    public void Save(HolidayMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday milestones. Empty fields match all.</summary>
public sealed record HolidayMilestoneQuery(string Locale, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayMilestone item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidayMilestone> Apply(List<HolidayMilestone> items)
    {
        var matched = Collections.Filter(items, (HolidayMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday milestone store.</summary>
public static class HolidayMilestoneStore
{
    public static IHolidayMilestoneStore NewMemoryHolidayMilestoneStore()
    {
        return new MemoryHolidayMilestoneStore(new Dictionary<string, HolidayMilestone>(), new List<string>());
    }

    public static IHolidayMilestoneStore NewCachedHolidayMilestoneStore(MemoryHolidayMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayMilestoneStore(inner, HolidayMilestoneCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<HolidayMilestone> FindHolidayMilestones(IHolidayMilestoneStore store, HolidayMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
