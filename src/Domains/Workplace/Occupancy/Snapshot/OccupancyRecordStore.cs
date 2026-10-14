namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy records by code.</summary>
public interface IOccupancyRecordStore
{
    OccupancyRecord? Find(string code);
    void Save(OccupancyRecord item);
    bool Remove(string code);
    List<OccupancyRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyRecordStore(Dictionary<string, OccupancyRecord> items, List<string> order) : IOccupancyRecordStore
{
    public Dictionary<string, OccupancyRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyRecord item)
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

    public List<OccupancyRecord> List()
    {
        var outValue = new List<OccupancyRecord>();
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
public sealed class CachedOccupancyRecordStore(MemoryOccupancyRecordStore inner, OccupancyRecordCache cache, int hits, int misses) : IOccupancyRecordStore
{
    public MemoryOccupancyRecordStore Inner { get; set; } = inner;
    public OccupancyRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyRecord? Find(string code)
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

    public void Save(OccupancyRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy records. Empty fields match all.</summary>
public sealed record OccupancyRecordQuery(string Department, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyRecord item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<OccupancyRecord> Apply(List<OccupancyRecord> items)
    {
        var matched = Collections.Filter(items, (OccupancyRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy record store.</summary>
public static class OccupancyRecordStore
{
    public static IOccupancyRecordStore NewMemoryOccupancyRecordStore()
    {
        return new MemoryOccupancyRecordStore(new Dictionary<string, OccupancyRecord>(), new List<string>());
    }

    public static IOccupancyRecordStore NewCachedOccupancyRecordStore(MemoryOccupancyRecordStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyRecordStore(inner, OccupancyRecordCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<OccupancyRecord> FindOccupancyRecords(IOccupancyRecordStore store, OccupancyRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
