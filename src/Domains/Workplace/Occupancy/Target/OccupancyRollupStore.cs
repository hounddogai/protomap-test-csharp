namespace Acme.Workforce.Workplace.Occupancy.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy rollups by code.</summary>
public interface IOccupancyRollupStore
{
    OccupancyRollup? Find(string code);
    void Save(OccupancyRollup item);
    bool Remove(string code);
    List<OccupancyRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyRollupStore(Dictionary<string, OccupancyRollup> items, List<string> order) : IOccupancyRollupStore
{
    public Dictionary<string, OccupancyRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyRollup item)
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

    public List<OccupancyRollup> List()
    {
        var outValue = new List<OccupancyRollup>();
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
public sealed class CachedOccupancyRollupStore(MemoryOccupancyRollupStore inner, OccupancyRollupCache cache, int hits, int misses) : IOccupancyRollupStore
{
    public MemoryOccupancyRollupStore Inner { get; set; } = inner;
    public OccupancyRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyRollup? Find(string code)
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

    public void Save(OccupancyRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy rollups. Empty fields match all.</summary>
public sealed record OccupancyRollupQuery(string Label, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyRollup item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<OccupancyRollup> Apply(List<OccupancyRollup> items)
    {
        var matched = Collections.Filter(items, (OccupancyRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy rollup store.</summary>
public static class OccupancyRollupStore
{
    public static IOccupancyRollupStore NewMemoryOccupancyRollupStore()
    {
        return new MemoryOccupancyRollupStore(new Dictionary<string, OccupancyRollup>(), new List<string>());
    }

    public static IOccupancyRollupStore NewCachedOccupancyRollupStore(MemoryOccupancyRollupStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyRollupStore(inner, OccupancyRollupCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<OccupancyRollup> FindOccupancyRollups(IOccupancyRollupStore store, OccupancyRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
