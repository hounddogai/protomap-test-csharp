namespace Acme.Workforce.Workplace.Occupancy.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default occupancy snapshots by code.</summary>
public interface IDefaultOccupancySnapshotStore
{
    DefaultOccupancySnapshot? Find(string code);
    void Save(DefaultOccupancySnapshot item);
    bool Remove(string code);
    List<DefaultOccupancySnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultOccupancySnapshotStore(Dictionary<string, DefaultOccupancySnapshot> items, List<string> order) : IDefaultOccupancySnapshotStore
{
    public Dictionary<string, DefaultOccupancySnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultOccupancySnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultOccupancySnapshot item)
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

    public List<DefaultOccupancySnapshot> List()
    {
        var outValue = new List<DefaultOccupancySnapshot>();
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
public sealed class CachedDefaultOccupancySnapshotStore(MemoryDefaultOccupancySnapshotStore inner, DefaultOccupancySnapshotCache cache, int hits, int misses) : IDefaultOccupancySnapshotStore
{
    public MemoryDefaultOccupancySnapshotStore Inner { get; set; } = inner;
    public DefaultOccupancySnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultOccupancySnapshot? Find(string code)
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

    public void Save(DefaultOccupancySnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultOccupancySnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default occupancy snapshots. Empty fields match all.</summary>
public sealed record DefaultOccupancySnapshotQuery(string Category, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultOccupancySnapshot item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultOccupancySnapshot> Apply(List<DefaultOccupancySnapshot> items)
    {
        var matched = Collections.Filter(items, (DefaultOccupancySnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default occupancy snapshot store.</summary>
public static class DefaultOccupancySnapshotStore
{
    public static IDefaultOccupancySnapshotStore NewMemoryDefaultOccupancySnapshotStore()
    {
        return new MemoryDefaultOccupancySnapshotStore(
            new Dictionary<string, DefaultOccupancySnapshot>(),
            new List<string>());
    }

    public static IDefaultOccupancySnapshotStore NewCachedDefaultOccupancySnapshotStore(MemoryDefaultOccupancySnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultOccupancySnapshotStore(
            inner,
            DefaultOccupancySnapshotCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<DefaultOccupancySnapshot> FindDefaultOccupancySnapshots(IDefaultOccupancySnapshotStore store, DefaultOccupancySnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
