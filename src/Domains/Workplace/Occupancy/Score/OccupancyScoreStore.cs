namespace Acme.Workforce.Workplace.Occupancy.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy scores by code.</summary>
public interface IOccupancyScoreStore
{
    OccupancyScore? Find(string code);
    void Save(OccupancyScore item);
    bool Remove(string code);
    List<OccupancyScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyScoreStore(Dictionary<string, OccupancyScore> items, List<string> order) : IOccupancyScoreStore
{
    public Dictionary<string, OccupancyScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyScore item)
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

    public List<OccupancyScore> List()
    {
        var outValue = new List<OccupancyScore>();
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
public sealed class CachedOccupancyScoreStore(MemoryOccupancyScoreStore inner, OccupancyScoreCache cache, int hits, int misses) : IOccupancyScoreStore
{
    public MemoryOccupancyScoreStore Inner { get; set; } = inner;
    public OccupancyScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyScore? Find(string code)
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

    public void Save(OccupancyScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy scores. Empty fields match all.</summary>
public sealed record OccupancyScoreQuery(string Name, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyScore item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OccupancyScore> Apply(List<OccupancyScore> items)
    {
        var matched = Collections.Filter(items, (OccupancyScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy score store.</summary>
public static class OccupancyScoreStore
{
    public static IOccupancyScoreStore NewMemoryOccupancyScoreStore()
    {
        return new MemoryOccupancyScoreStore(new Dictionary<string, OccupancyScore>(), new List<string>());
    }

    public static IOccupancyScoreStore NewCachedOccupancyScoreStore(MemoryOccupancyScoreStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyScoreStore(inner, OccupancyScoreCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<OccupancyScore> FindOccupancyScores(IOccupancyScoreStore store, OccupancyScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
