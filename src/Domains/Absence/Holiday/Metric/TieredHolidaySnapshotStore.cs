namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered holiday snapshots by code.</summary>
public interface ITieredHolidaySnapshotStore
{
    TieredHolidaySnapshot? Find(string code);
    void Save(TieredHolidaySnapshot item);
    bool Remove(string code);
    List<TieredHolidaySnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredHolidaySnapshotStore(Dictionary<string, TieredHolidaySnapshot> items, List<string> order) : ITieredHolidaySnapshotStore
{
    public Dictionary<string, TieredHolidaySnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredHolidaySnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredHolidaySnapshot item)
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

    public List<TieredHolidaySnapshot> List()
    {
        var outValue = new List<TieredHolidaySnapshot>();
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
public sealed class CachedTieredHolidaySnapshotStore(MemoryTieredHolidaySnapshotStore inner, TieredHolidaySnapshotCache cache, int hits, int misses) : ITieredHolidaySnapshotStore
{
    public MemoryTieredHolidaySnapshotStore Inner { get; set; } = inner;
    public TieredHolidaySnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredHolidaySnapshot? Find(string code)
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

    public void Save(TieredHolidaySnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredHolidaySnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered holiday snapshots. Empty fields match all.</summary>
public sealed record TieredHolidaySnapshotQuery(string Name, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(TieredHolidaySnapshot item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredHolidaySnapshot> Apply(List<TieredHolidaySnapshot> items)
    {
        var matched = Collections.Filter(items, (TieredHolidaySnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered holiday snapshot store.</summary>
public static class TieredHolidaySnapshotStore
{
    public static ITieredHolidaySnapshotStore NewMemoryTieredHolidaySnapshotStore()
    {
        return new MemoryTieredHolidaySnapshotStore(
            new Dictionary<string, TieredHolidaySnapshot>(),
            new List<string>());
    }

    public static ITieredHolidaySnapshotStore NewCachedTieredHolidaySnapshotStore(MemoryTieredHolidaySnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedTieredHolidaySnapshotStore(inner, TieredHolidaySnapshotCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TieredHolidaySnapshot> FindTieredHolidaySnapshots(ITieredHolidaySnapshotStore store, TieredHolidaySnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
