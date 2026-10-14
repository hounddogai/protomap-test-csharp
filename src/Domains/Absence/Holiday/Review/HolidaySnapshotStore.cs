namespace Acme.Workforce.Absence.Holiday.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday snapshots by code.</summary>
public interface IHolidaySnapshotStore
{
    HolidaySnapshot? Find(string code);
    void Save(HolidaySnapshot item);
    bool Remove(string code);
    List<HolidaySnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidaySnapshotStore(Dictionary<string, HolidaySnapshot> items, List<string> order) : IHolidaySnapshotStore
{
    public Dictionary<string, HolidaySnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidaySnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidaySnapshot item)
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

    public List<HolidaySnapshot> List()
    {
        var outValue = new List<HolidaySnapshot>();
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
public sealed class CachedHolidaySnapshotStore(MemoryHolidaySnapshotStore inner, HolidaySnapshotCache cache, int hits, int misses) : IHolidaySnapshotStore
{
    public MemoryHolidaySnapshotStore Inner { get; set; } = inner;
    public HolidaySnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidaySnapshot? Find(string code)
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

    public void Save(HolidaySnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidaySnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday snapshots. Empty fields match all.</summary>
public sealed record HolidaySnapshotQuery(string Country, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(HolidaySnapshot item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidaySnapshot> Apply(List<HolidaySnapshot> items)
    {
        var matched = Collections.Filter(items, (HolidaySnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday snapshot store.</summary>
public static class HolidaySnapshotStore
{
    public static IHolidaySnapshotStore NewMemoryHolidaySnapshotStore()
    {
        return new MemoryHolidaySnapshotStore(new Dictionary<string, HolidaySnapshot>(), new List<string>());
    }

    public static IHolidaySnapshotStore NewCachedHolidaySnapshotStore(MemoryHolidaySnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedHolidaySnapshotStore(inner, HolidaySnapshotCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<HolidaySnapshot> FindHolidaySnapshots(IHolidaySnapshotStore store, HolidaySnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
