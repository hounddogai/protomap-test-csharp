namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default holiday milestones by code.</summary>
public interface IDefaultHolidayMilestoneStore
{
    DefaultHolidayMilestone? Find(string code);
    void Save(DefaultHolidayMilestone item);
    bool Remove(string code);
    List<DefaultHolidayMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultHolidayMilestoneStore(Dictionary<string, DefaultHolidayMilestone> items, List<string> order) : IDefaultHolidayMilestoneStore
{
    public Dictionary<string, DefaultHolidayMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultHolidayMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultHolidayMilestone item)
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

    public List<DefaultHolidayMilestone> List()
    {
        var outValue = new List<DefaultHolidayMilestone>();
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
public sealed class CachedDefaultHolidayMilestoneStore(MemoryDefaultHolidayMilestoneStore inner, DefaultHolidayMilestoneCache cache, int hits, int misses) : IDefaultHolidayMilestoneStore
{
    public MemoryDefaultHolidayMilestoneStore Inner { get; set; } = inner;
    public DefaultHolidayMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultHolidayMilestone? Find(string code)
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

    public void Save(DefaultHolidayMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultHolidayMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default holiday milestones. Empty fields match all.</summary>
public sealed record DefaultHolidayMilestoneQuery(string Label, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultHolidayMilestone item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultHolidayMilestone> Apply(List<DefaultHolidayMilestone> items)
    {
        var matched = Collections.Filter(items, (DefaultHolidayMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default holiday milestone store.</summary>
public static class DefaultHolidayMilestoneStore
{
    public static IDefaultHolidayMilestoneStore NewMemoryDefaultHolidayMilestoneStore()
    {
        return new MemoryDefaultHolidayMilestoneStore(
            new Dictionary<string, DefaultHolidayMilestone>(),
            new List<string>());
    }

    public static IDefaultHolidayMilestoneStore NewCachedDefaultHolidayMilestoneStore(MemoryDefaultHolidayMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultHolidayMilestoneStore(
            inner,
            DefaultHolidayMilestoneCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<DefaultHolidayMilestone> FindDefaultHolidayMilestones(IDefaultHolidayMilestoneStore store, DefaultHolidayMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
