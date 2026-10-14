namespace Acme.Workforce.Workplace.FloorPlan.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled floor plan entries by code.</summary>
public interface IScheduledFloorPlanEntryStore
{
    ScheduledFloorPlanEntry? Find(string code);
    void Save(ScheduledFloorPlanEntry item);
    bool Remove(string code);
    List<ScheduledFloorPlanEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledFloorPlanEntryStore(Dictionary<string, ScheduledFloorPlanEntry> items, List<string> order) : IScheduledFloorPlanEntryStore
{
    public Dictionary<string, ScheduledFloorPlanEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledFloorPlanEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledFloorPlanEntry item)
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

    public List<ScheduledFloorPlanEntry> List()
    {
        var outValue = new List<ScheduledFloorPlanEntry>();
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
public sealed class CachedScheduledFloorPlanEntryStore(MemoryScheduledFloorPlanEntryStore inner, ScheduledFloorPlanEntryCache cache, int hits, int misses) : IScheduledFloorPlanEntryStore
{
    public MemoryScheduledFloorPlanEntryStore Inner { get; set; } = inner;
    public ScheduledFloorPlanEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledFloorPlanEntry? Find(string code)
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

    public void Save(ScheduledFloorPlanEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledFloorPlanEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled floor plan entries. Empty fields match all.</summary>
public sealed record ScheduledFloorPlanEntryQuery(string Segment, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledFloorPlanEntry item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledFloorPlanEntry> Apply(List<ScheduledFloorPlanEntry> items)
    {
        var matched = Collections.Filter(items, (ScheduledFloorPlanEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled floor plan entry store.</summary>
public static class ScheduledFloorPlanEntryStore
{
    public static IScheduledFloorPlanEntryStore NewMemoryScheduledFloorPlanEntryStore()
    {
        return new MemoryScheduledFloorPlanEntryStore(
            new Dictionary<string, ScheduledFloorPlanEntry>(),
            new List<string>());
    }

    public static IScheduledFloorPlanEntryStore NewCachedScheduledFloorPlanEntryStore(MemoryScheduledFloorPlanEntryStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledFloorPlanEntryStore(
            inner,
            ScheduledFloorPlanEntryCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledFloorPlanEntry> FindScheduledFloorPlanEntries(IScheduledFloorPlanEntryStore store, ScheduledFloorPlanEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
