namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending reorg snapshots by code.</summary>
public interface IPendingReorgSnapshotStore
{
    PendingReorgSnapshot? Find(string code);
    void Save(PendingReorgSnapshot item);
    bool Remove(string code);
    List<PendingReorgSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingReorgSnapshotStore(Dictionary<string, PendingReorgSnapshot> items, List<string> order) : IPendingReorgSnapshotStore
{
    public Dictionary<string, PendingReorgSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingReorgSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingReorgSnapshot item)
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

    public List<PendingReorgSnapshot> List()
    {
        var outValue = new List<PendingReorgSnapshot>();
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
public sealed class CachedPendingReorgSnapshotStore(MemoryPendingReorgSnapshotStore inner, PendingReorgSnapshotCache cache, int hits, int misses) : IPendingReorgSnapshotStore
{
    public MemoryPendingReorgSnapshotStore Inner { get; set; } = inner;
    public PendingReorgSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingReorgSnapshot? Find(string code)
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

    public void Save(PendingReorgSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingReorgSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending reorg snapshots. Empty fields match all.</summary>
public sealed record PendingReorgSnapshotQuery(string Country, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(PendingReorgSnapshot item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingReorgSnapshot> Apply(List<PendingReorgSnapshot> items)
    {
        var matched = Collections.Filter(items, (PendingReorgSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending reorg snapshot store.</summary>
public static class PendingReorgSnapshotStore
{
    public static IPendingReorgSnapshotStore NewMemoryPendingReorgSnapshotStore()
    {
        return new MemoryPendingReorgSnapshotStore(
            new Dictionary<string, PendingReorgSnapshot>(),
            new List<string>());
    }

    public static IPendingReorgSnapshotStore NewCachedPendingReorgSnapshotStore(MemoryPendingReorgSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedPendingReorgSnapshotStore(inner, PendingReorgSnapshotCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PendingReorgSnapshot> FindPendingReorgSnapshots(IPendingReorgSnapshotStore store, PendingReorgSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
