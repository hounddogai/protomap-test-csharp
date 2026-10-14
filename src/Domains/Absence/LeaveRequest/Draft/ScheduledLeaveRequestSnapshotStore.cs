namespace Acme.Workforce.Absence.LeaveRequest.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled leave request snapshots by code.</summary>
public interface IScheduledLeaveRequestSnapshotStore
{
    ScheduledLeaveRequestSnapshot? Find(string code);
    void Save(ScheduledLeaveRequestSnapshot item);
    bool Remove(string code);
    List<ScheduledLeaveRequestSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledLeaveRequestSnapshotStore(Dictionary<string, ScheduledLeaveRequestSnapshot> items, List<string> order) : IScheduledLeaveRequestSnapshotStore
{
    public Dictionary<string, ScheduledLeaveRequestSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledLeaveRequestSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledLeaveRequestSnapshot item)
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

    public List<ScheduledLeaveRequestSnapshot> List()
    {
        var outValue = new List<ScheduledLeaveRequestSnapshot>();
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
public sealed class CachedScheduledLeaveRequestSnapshotStore(MemoryScheduledLeaveRequestSnapshotStore inner, ScheduledLeaveRequestSnapshotCache cache, int hits, int misses) : IScheduledLeaveRequestSnapshotStore
{
    public MemoryScheduledLeaveRequestSnapshotStore Inner { get; set; } = inner;
    public ScheduledLeaveRequestSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledLeaveRequestSnapshot? Find(string code)
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

    public void Save(ScheduledLeaveRequestSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledLeaveRequestSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled leave request snapshots. Empty fields match all.</summary>
public sealed record ScheduledLeaveRequestSnapshotQuery(string Name, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledLeaveRequestSnapshot item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledLeaveRequestSnapshot> Apply(List<ScheduledLeaveRequestSnapshot> items)
    {
        var matched = Collections.Filter(items, (ScheduledLeaveRequestSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled leave request snapshot store.</summary>
public static class ScheduledLeaveRequestSnapshotStore
{
    public static IScheduledLeaveRequestSnapshotStore NewMemoryScheduledLeaveRequestSnapshotStore()
    {
        return new MemoryScheduledLeaveRequestSnapshotStore(
            new Dictionary<string, ScheduledLeaveRequestSnapshot>(),
            new List<string>());
    }

    public static IScheduledLeaveRequestSnapshotStore NewCachedScheduledLeaveRequestSnapshotStore(MemoryScheduledLeaveRequestSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledLeaveRequestSnapshotStore(
            inner,
            ScheduledLeaveRequestSnapshotCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledLeaveRequestSnapshot> FindScheduledLeaveRequestSnapshots(IScheduledLeaveRequestSnapshotStore store, ScheduledLeaveRequestSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
