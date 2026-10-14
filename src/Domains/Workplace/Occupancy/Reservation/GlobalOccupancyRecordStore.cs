namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global occupancy records by code.</summary>
public interface IGlobalOccupancyRecordStore
{
    GlobalOccupancyRecord? Find(string code);
    void Save(GlobalOccupancyRecord item);
    bool Remove(string code);
    List<GlobalOccupancyRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalOccupancyRecordStore(Dictionary<string, GlobalOccupancyRecord> items, List<string> order) : IGlobalOccupancyRecordStore
{
    public Dictionary<string, GlobalOccupancyRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalOccupancyRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalOccupancyRecord item)
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

    public List<GlobalOccupancyRecord> List()
    {
        var outValue = new List<GlobalOccupancyRecord>();
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
public sealed class CachedGlobalOccupancyRecordStore(MemoryGlobalOccupancyRecordStore inner, GlobalOccupancyRecordCache cache, int hits, int misses) : IGlobalOccupancyRecordStore
{
    public MemoryGlobalOccupancyRecordStore Inner { get; set; } = inner;
    public GlobalOccupancyRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalOccupancyRecord? Find(string code)
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

    public void Save(GlobalOccupancyRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalOccupancyRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global occupancy records. Empty fields match all.</summary>
public sealed record GlobalOccupancyRecordQuery(string Name, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalOccupancyRecord item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalOccupancyRecord> Apply(List<GlobalOccupancyRecord> items)
    {
        var matched = Collections.Filter(items, (GlobalOccupancyRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global occupancy record store.</summary>
public static class GlobalOccupancyRecordStore
{
    public static IGlobalOccupancyRecordStore NewMemoryGlobalOccupancyRecordStore()
    {
        return new MemoryGlobalOccupancyRecordStore(
            new Dictionary<string, GlobalOccupancyRecord>(),
            new List<string>());
    }

    public static IGlobalOccupancyRecordStore NewCachedGlobalOccupancyRecordStore(MemoryGlobalOccupancyRecordStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalOccupancyRecordStore(inner, GlobalOccupancyRecordCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<GlobalOccupancyRecord> FindGlobalOccupancyRecords(IGlobalOccupancyRecordStore store, GlobalOccupancyRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
