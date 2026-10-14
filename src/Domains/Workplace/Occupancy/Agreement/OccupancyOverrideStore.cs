namespace Acme.Workforce.Workplace.Occupancy.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy overrides by code.</summary>
public interface IOccupancyOverrideStore
{
    OccupancyOverride? Find(string code);
    void Save(OccupancyOverride item);
    bool Remove(string code);
    List<OccupancyOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyOverrideStore(Dictionary<string, OccupancyOverride> items, List<string> order) : IOccupancyOverrideStore
{
    public Dictionary<string, OccupancyOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyOverride item)
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

    public List<OccupancyOverride> List()
    {
        var outValue = new List<OccupancyOverride>();
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
public sealed class CachedOccupancyOverrideStore(MemoryOccupancyOverrideStore inner, OccupancyOverrideCache cache, int hits, int misses) : IOccupancyOverrideStore
{
    public MemoryOccupancyOverrideStore Inner { get; set; } = inner;
    public OccupancyOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyOverride? Find(string code)
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

    public void Save(OccupancyOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy overrides. Empty fields match all.</summary>
public sealed record OccupancyOverrideQuery(string Name, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyOverride item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OccupancyOverride> Apply(List<OccupancyOverride> items)
    {
        var matched = Collections.Filter(items, (OccupancyOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy override store.</summary>
public static class OccupancyOverrideStore
{
    public static IOccupancyOverrideStore NewMemoryOccupancyOverrideStore()
    {
        return new MemoryOccupancyOverrideStore(new Dictionary<string, OccupancyOverride>(), new List<string>());
    }

    public static IOccupancyOverrideStore NewCachedOccupancyOverrideStore(MemoryOccupancyOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyOverrideStore(inner, OccupancyOverrideCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<OccupancyOverride> FindOccupancyOverrides(IOccupancyOverrideStore store, OccupancyOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
