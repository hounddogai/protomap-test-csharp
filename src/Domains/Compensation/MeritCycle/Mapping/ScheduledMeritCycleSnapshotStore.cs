namespace Acme.Workforce.Compensation.MeritCycle.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled merit cycle snapshots by code.</summary>
public interface IScheduledMeritCycleSnapshotStore
{
    ScheduledMeritCycleSnapshot? Find(string code);
    void Save(ScheduledMeritCycleSnapshot item);
    bool Remove(string code);
    List<ScheduledMeritCycleSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledMeritCycleSnapshotStore(Dictionary<string, ScheduledMeritCycleSnapshot> items, List<string> order) : IScheduledMeritCycleSnapshotStore
{
    public Dictionary<string, ScheduledMeritCycleSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledMeritCycleSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledMeritCycleSnapshot item)
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

    public List<ScheduledMeritCycleSnapshot> List()
    {
        var outValue = new List<ScheduledMeritCycleSnapshot>();
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
public sealed class CachedScheduledMeritCycleSnapshotStore(MemoryScheduledMeritCycleSnapshotStore inner, ScheduledMeritCycleSnapshotCache cache, int hits, int misses) : IScheduledMeritCycleSnapshotStore
{
    public MemoryScheduledMeritCycleSnapshotStore Inner { get; set; } = inner;
    public ScheduledMeritCycleSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledMeritCycleSnapshot? Find(string code)
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

    public void Save(ScheduledMeritCycleSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledMeritCycleSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled merit cycle snapshots. Empty fields match all.</summary>
public sealed record ScheduledMeritCycleSnapshotQuery(string Team, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledMeritCycleSnapshot item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<ScheduledMeritCycleSnapshot> Apply(List<ScheduledMeritCycleSnapshot> items)
    {
        var matched = Collections.Filter(items, (ScheduledMeritCycleSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled merit cycle snapshot store.</summary>
public static class ScheduledMeritCycleSnapshotStore
{
    public static IScheduledMeritCycleSnapshotStore NewMemoryScheduledMeritCycleSnapshotStore()
    {
        return new MemoryScheduledMeritCycleSnapshotStore(
            new Dictionary<string, ScheduledMeritCycleSnapshot>(),
            new List<string>());
    }

    public static IScheduledMeritCycleSnapshotStore NewCachedScheduledMeritCycleSnapshotStore(MemoryScheduledMeritCycleSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledMeritCycleSnapshotStore(
            inner,
            ScheduledMeritCycleSnapshotCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledMeritCycleSnapshot> FindScheduledMeritCycleSnapshots(IScheduledMeritCycleSnapshotStore store, ScheduledMeritCycleSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
