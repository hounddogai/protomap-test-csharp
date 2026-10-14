namespace Acme.Workforce.Engagement.Recognition.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled recognition snapshots by code.</summary>
public interface IScheduledRecognitionSnapshotStore
{
    ScheduledRecognitionSnapshot? Find(string code);
    void Save(ScheduledRecognitionSnapshot item);
    bool Remove(string code);
    List<ScheduledRecognitionSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledRecognitionSnapshotStore(Dictionary<string, ScheduledRecognitionSnapshot> items, List<string> order) : IScheduledRecognitionSnapshotStore
{
    public Dictionary<string, ScheduledRecognitionSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledRecognitionSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledRecognitionSnapshot item)
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

    public List<ScheduledRecognitionSnapshot> List()
    {
        var outValue = new List<ScheduledRecognitionSnapshot>();
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
public sealed class CachedScheduledRecognitionSnapshotStore(MemoryScheduledRecognitionSnapshotStore inner, ScheduledRecognitionSnapshotCache cache, int hits, int misses) : IScheduledRecognitionSnapshotStore
{
    public MemoryScheduledRecognitionSnapshotStore Inner { get; set; } = inner;
    public ScheduledRecognitionSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledRecognitionSnapshot? Find(string code)
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

    public void Save(ScheduledRecognitionSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledRecognitionSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled recognition snapshots. Empty fields match all.</summary>
public sealed record ScheduledRecognitionSnapshotQuery(string Region, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledRecognitionSnapshot item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledRecognitionSnapshot> Apply(List<ScheduledRecognitionSnapshot> items)
    {
        var matched = Collections.Filter(items, (ScheduledRecognitionSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled recognition snapshot store.</summary>
public static class ScheduledRecognitionSnapshotStore
{
    public static IScheduledRecognitionSnapshotStore NewMemoryScheduledRecognitionSnapshotStore()
    {
        return new MemoryScheduledRecognitionSnapshotStore(
            new Dictionary<string, ScheduledRecognitionSnapshot>(),
            new List<string>());
    }

    public static IScheduledRecognitionSnapshotStore NewCachedScheduledRecognitionSnapshotStore(MemoryScheduledRecognitionSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledRecognitionSnapshotStore(
            inner,
            ScheduledRecognitionSnapshotCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledRecognitionSnapshot> FindScheduledRecognitionSnapshots(IScheduledRecognitionSnapshotStore store, ScheduledRecognitionSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
