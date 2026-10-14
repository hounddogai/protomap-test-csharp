namespace Acme.Workforce.Workplace.Occupancy.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy milestones by code.</summary>
public interface IOccupancyMilestoneStore
{
    OccupancyMilestone? Find(string code);
    void Save(OccupancyMilestone item);
    bool Remove(string code);
    List<OccupancyMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyMilestoneStore(Dictionary<string, OccupancyMilestone> items, List<string> order) : IOccupancyMilestoneStore
{
    public Dictionary<string, OccupancyMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyMilestone item)
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

    public List<OccupancyMilestone> List()
    {
        var outValue = new List<OccupancyMilestone>();
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
public sealed class CachedOccupancyMilestoneStore(MemoryOccupancyMilestoneStore inner, OccupancyMilestoneCache cache, int hits, int misses) : IOccupancyMilestoneStore
{
    public MemoryOccupancyMilestoneStore Inner { get; set; } = inner;
    public OccupancyMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyMilestone? Find(string code)
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

    public void Save(OccupancyMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy milestones. Empty fields match all.</summary>
public sealed record OccupancyMilestoneQuery(string Name, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyMilestone item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OccupancyMilestone> Apply(List<OccupancyMilestone> items)
    {
        var matched = Collections.Filter(items, (OccupancyMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy milestone store.</summary>
public static class OccupancyMilestoneStore
{
    public static IOccupancyMilestoneStore NewMemoryOccupancyMilestoneStore()
    {
        return new MemoryOccupancyMilestoneStore(new Dictionary<string, OccupancyMilestone>(), new List<string>());
    }

    public static IOccupancyMilestoneStore NewCachedOccupancyMilestoneStore(MemoryOccupancyMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyMilestoneStore(inner, OccupancyMilestoneCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<OccupancyMilestone> FindOccupancyMilestones(IOccupancyMilestoneStore store, OccupancyMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
