namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility snapshots by code.</summary>
public interface IMobilitySnapshotStore
{
    MobilitySnapshot? Find(string code);
    void Save(MobilitySnapshot item);
    bool Remove(string code);
    List<MobilitySnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilitySnapshotStore(Dictionary<string, MobilitySnapshot> items, List<string> order) : IMobilitySnapshotStore
{
    public Dictionary<string, MobilitySnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilitySnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilitySnapshot item)
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

    public List<MobilitySnapshot> List()
    {
        var outValue = new List<MobilitySnapshot>();
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
public sealed class CachedMobilitySnapshotStore(MemoryMobilitySnapshotStore inner, MobilitySnapshotCache cache, int hits, int misses) : IMobilitySnapshotStore
{
    public MemoryMobilitySnapshotStore Inner { get; set; } = inner;
    public MobilitySnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilitySnapshot? Find(string code)
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

    public void Save(MobilitySnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilitySnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility snapshots. Empty fields match all.</summary>
public sealed record MobilitySnapshotQuery(string Unit, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(MobilitySnapshot item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<MobilitySnapshot> Apply(List<MobilitySnapshot> items)
    {
        var matched = Collections.Filter(items, (MobilitySnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility snapshot store.</summary>
public static class MobilitySnapshotStore
{
    public static IMobilitySnapshotStore NewMemoryMobilitySnapshotStore()
    {
        return new MemoryMobilitySnapshotStore(new Dictionary<string, MobilitySnapshot>(), new List<string>());
    }

    public static IMobilitySnapshotStore NewCachedMobilitySnapshotStore(MemoryMobilitySnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedMobilitySnapshotStore(inner, MobilitySnapshotCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<MobilitySnapshot> FindMobilitySnapshots(IMobilitySnapshotStore store, MobilitySnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
