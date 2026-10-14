namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy groups by code.</summary>
public interface IOccupancyGroupStore
{
    OccupancyGroup? Find(string code);
    void Save(OccupancyGroup item);
    bool Remove(string code);
    List<OccupancyGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyGroupStore(Dictionary<string, OccupancyGroup> items, List<string> order) : IOccupancyGroupStore
{
    public Dictionary<string, OccupancyGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyGroup item)
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

    public List<OccupancyGroup> List()
    {
        var outValue = new List<OccupancyGroup>();
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
public sealed class CachedOccupancyGroupStore(MemoryOccupancyGroupStore inner, OccupancyGroupCache cache, int hits, int misses) : IOccupancyGroupStore
{
    public MemoryOccupancyGroupStore Inner { get; set; } = inner;
    public OccupancyGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyGroup? Find(string code)
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

    public void Save(OccupancyGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy groups. Empty fields match all.</summary>
public sealed record OccupancyGroupQuery(string Name, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyGroup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OccupancyGroup> Apply(List<OccupancyGroup> items)
    {
        var matched = Collections.Filter(items, (OccupancyGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy group store.</summary>
public static class OccupancyGroupStore
{
    public static IOccupancyGroupStore NewMemoryOccupancyGroupStore()
    {
        return new MemoryOccupancyGroupStore(new Dictionary<string, OccupancyGroup>(), new List<string>());
    }

    public static IOccupancyGroupStore NewCachedOccupancyGroupStore(MemoryOccupancyGroupStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyGroupStore(inner, OccupancyGroupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<OccupancyGroup> FindOccupancyGroups(IOccupancyGroupStore store, OccupancyGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
