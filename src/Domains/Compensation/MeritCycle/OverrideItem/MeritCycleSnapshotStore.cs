namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle snapshots by code.</summary>
public interface IMeritCycleSnapshotStore
{
    MeritCycleSnapshot? Find(string code);
    void Save(MeritCycleSnapshot item);
    bool Remove(string code);
    List<MeritCycleSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleSnapshotStore(Dictionary<string, MeritCycleSnapshot> items, List<string> order) : IMeritCycleSnapshotStore
{
    public Dictionary<string, MeritCycleSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleSnapshot item)
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

    public List<MeritCycleSnapshot> List()
    {
        var outValue = new List<MeritCycleSnapshot>();
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
public sealed class CachedMeritCycleSnapshotStore(MemoryMeritCycleSnapshotStore inner, MeritCycleSnapshotCache cache, int hits, int misses) : IMeritCycleSnapshotStore
{
    public MemoryMeritCycleSnapshotStore Inner { get; set; } = inner;
    public MeritCycleSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleSnapshot? Find(string code)
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

    public void Save(MeritCycleSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle snapshots. Empty fields match all.</summary>
public sealed record MeritCycleSnapshotQuery(string Region, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleSnapshot item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleSnapshot> Apply(List<MeritCycleSnapshot> items)
    {
        var matched = Collections.Filter(items, (MeritCycleSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle snapshot store.</summary>
public static class MeritCycleSnapshotStore
{
    public static IMeritCycleSnapshotStore NewMemoryMeritCycleSnapshotStore()
    {
        return new MemoryMeritCycleSnapshotStore(new Dictionary<string, MeritCycleSnapshot>(), new List<string>());
    }

    public static IMeritCycleSnapshotStore NewCachedMeritCycleSnapshotStore(MemoryMeritCycleSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleSnapshotStore(inner, MeritCycleSnapshotCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<MeritCycleSnapshot> FindMeritCycleSnapshots(IMeritCycleSnapshotStore store, MeritCycleSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
