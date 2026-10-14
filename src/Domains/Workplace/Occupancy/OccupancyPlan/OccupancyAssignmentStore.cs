namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy assignments by code.</summary>
public interface IOccupancyAssignmentStore
{
    OccupancyAssignment? Find(string code);
    void Save(OccupancyAssignment item);
    bool Remove(string code);
    List<OccupancyAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyAssignmentStore(Dictionary<string, OccupancyAssignment> items, List<string> order) : IOccupancyAssignmentStore
{
    public Dictionary<string, OccupancyAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyAssignment item)
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

    public List<OccupancyAssignment> List()
    {
        var outValue = new List<OccupancyAssignment>();
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
public sealed class CachedOccupancyAssignmentStore(MemoryOccupancyAssignmentStore inner, OccupancyAssignmentCache cache, int hits, int misses) : IOccupancyAssignmentStore
{
    public MemoryOccupancyAssignmentStore Inner { get; set; } = inner;
    public OccupancyAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyAssignment? Find(string code)
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

    public void Save(OccupancyAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy assignments. Empty fields match all.</summary>
public sealed record OccupancyAssignmentQuery(string Unit, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyAssignment item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OccupancyAssignment> Apply(List<OccupancyAssignment> items)
    {
        var matched = Collections.Filter(items, (OccupancyAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy assignment store.</summary>
public static class OccupancyAssignmentStore
{
    public static IOccupancyAssignmentStore NewMemoryOccupancyAssignmentStore()
    {
        return new MemoryOccupancyAssignmentStore(new Dictionary<string, OccupancyAssignment>(), new List<string>());
    }

    public static IOccupancyAssignmentStore NewCachedOccupancyAssignmentStore(MemoryOccupancyAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyAssignmentStore(inner, OccupancyAssignmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<OccupancyAssignment> FindOccupancyAssignments(IOccupancyAssignmentStore store, OccupancyAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
