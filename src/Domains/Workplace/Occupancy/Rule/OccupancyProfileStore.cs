namespace Acme.Workforce.Workplace.Occupancy.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy profiles by code.</summary>
public interface IOccupancyProfileStore
{
    OccupancyProfile? Find(string code);
    void Save(OccupancyProfile item);
    bool Remove(string code);
    List<OccupancyProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyProfileStore(Dictionary<string, OccupancyProfile> items, List<string> order) : IOccupancyProfileStore
{
    public Dictionary<string, OccupancyProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyProfile item)
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

    public List<OccupancyProfile> List()
    {
        var outValue = new List<OccupancyProfile>();
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
public sealed class CachedOccupancyProfileStore(MemoryOccupancyProfileStore inner, OccupancyProfileCache cache, int hits, int misses) : IOccupancyProfileStore
{
    public MemoryOccupancyProfileStore Inner { get; set; } = inner;
    public OccupancyProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyProfile? Find(string code)
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

    public void Save(OccupancyProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy profiles. Empty fields match all.</summary>
public sealed record OccupancyProfileQuery(string Title, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyProfile item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<OccupancyProfile> Apply(List<OccupancyProfile> items)
    {
        var matched = Collections.Filter(items, (OccupancyProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy profile store.</summary>
public static class OccupancyProfileStore
{
    public static IOccupancyProfileStore NewMemoryOccupancyProfileStore()
    {
        return new MemoryOccupancyProfileStore(new Dictionary<string, OccupancyProfile>(), new List<string>());
    }

    public static IOccupancyProfileStore NewCachedOccupancyProfileStore(MemoryOccupancyProfileStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyProfileStore(inner, OccupancyProfileCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<OccupancyProfile> FindOccupancyProfiles(IOccupancyProfileStore store, OccupancyProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
