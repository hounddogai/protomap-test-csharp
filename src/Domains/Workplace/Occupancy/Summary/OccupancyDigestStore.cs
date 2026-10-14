namespace Acme.Workforce.Workplace.Occupancy.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy digests by code.</summary>
public interface IOccupancyDigestStore
{
    OccupancyDigest? Find(string code);
    void Save(OccupancyDigest item);
    bool Remove(string code);
    List<OccupancyDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyDigestStore(Dictionary<string, OccupancyDigest> items, List<string> order) : IOccupancyDigestStore
{
    public Dictionary<string, OccupancyDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyDigest item)
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

    public List<OccupancyDigest> List()
    {
        var outValue = new List<OccupancyDigest>();
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
public sealed class CachedOccupancyDigestStore(MemoryOccupancyDigestStore inner, OccupancyDigestCache cache, int hits, int misses) : IOccupancyDigestStore
{
    public MemoryOccupancyDigestStore Inner { get; set; } = inner;
    public OccupancyDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyDigest? Find(string code)
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

    public void Save(OccupancyDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy digests. Empty fields match all.</summary>
public sealed record OccupancyDigestQuery(string Country, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyDigest item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OccupancyDigest> Apply(List<OccupancyDigest> items)
    {
        var matched = Collections.Filter(items, (OccupancyDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy digest store.</summary>
public static class OccupancyDigestStore
{
    public static IOccupancyDigestStore NewMemoryOccupancyDigestStore()
    {
        return new MemoryOccupancyDigestStore(new Dictionary<string, OccupancyDigest>(), new List<string>());
    }

    public static IOccupancyDigestStore NewCachedOccupancyDigestStore(MemoryOccupancyDigestStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyDigestStore(inner, OccupancyDigestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<OccupancyDigest> FindOccupancyDigests(IOccupancyDigestStore store, OccupancyDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
