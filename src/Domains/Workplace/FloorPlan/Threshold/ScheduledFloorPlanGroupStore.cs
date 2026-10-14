namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled floor plan groups by code.</summary>
public interface IScheduledFloorPlanGroupStore
{
    ScheduledFloorPlanGroup? Find(string code);
    void Save(ScheduledFloorPlanGroup item);
    bool Remove(string code);
    List<ScheduledFloorPlanGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledFloorPlanGroupStore(Dictionary<string, ScheduledFloorPlanGroup> items, List<string> order) : IScheduledFloorPlanGroupStore
{
    public Dictionary<string, ScheduledFloorPlanGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledFloorPlanGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledFloorPlanGroup item)
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

    public List<ScheduledFloorPlanGroup> List()
    {
        var outValue = new List<ScheduledFloorPlanGroup>();
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
public sealed class CachedScheduledFloorPlanGroupStore(MemoryScheduledFloorPlanGroupStore inner, ScheduledFloorPlanGroupCache cache, int hits, int misses) : IScheduledFloorPlanGroupStore
{
    public MemoryScheduledFloorPlanGroupStore Inner { get; set; } = inner;
    public ScheduledFloorPlanGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledFloorPlanGroup? Find(string code)
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

    public void Save(ScheduledFloorPlanGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledFloorPlanGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled floor plan groups. Empty fields match all.</summary>
public sealed record ScheduledFloorPlanGroupQuery(string Title, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledFloorPlanGroup item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledFloorPlanGroup> Apply(List<ScheduledFloorPlanGroup> items)
    {
        var matched = Collections.Filter(items, (ScheduledFloorPlanGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled floor plan group store.</summary>
public static class ScheduledFloorPlanGroupStore
{
    public static IScheduledFloorPlanGroupStore NewMemoryScheduledFloorPlanGroupStore()
    {
        return new MemoryScheduledFloorPlanGroupStore(
            new Dictionary<string, ScheduledFloorPlanGroup>(),
            new List<string>());
    }

    public static IScheduledFloorPlanGroupStore NewCachedScheduledFloorPlanGroupStore(MemoryScheduledFloorPlanGroupStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledFloorPlanGroupStore(
            inner,
            ScheduledFloorPlanGroupCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledFloorPlanGroup> FindScheduledFloorPlanGroups(IScheduledFloorPlanGroupStore store, ScheduledFloorPlanGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
