namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered assignment snapshots by code.</summary>
public interface ITieredAssignmentSnapshotStore
{
    TieredAssignmentSnapshot? Find(string code);
    void Save(TieredAssignmentSnapshot item);
    bool Remove(string code);
    List<TieredAssignmentSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredAssignmentSnapshotStore(Dictionary<string, TieredAssignmentSnapshot> items, List<string> order) : ITieredAssignmentSnapshotStore
{
    public Dictionary<string, TieredAssignmentSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredAssignmentSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredAssignmentSnapshot item)
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

    public List<TieredAssignmentSnapshot> List()
    {
        var outValue = new List<TieredAssignmentSnapshot>();
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
public sealed class CachedTieredAssignmentSnapshotStore(MemoryTieredAssignmentSnapshotStore inner, TieredAssignmentSnapshotCache cache, int hits, int misses) : ITieredAssignmentSnapshotStore
{
    public MemoryTieredAssignmentSnapshotStore Inner { get; set; } = inner;
    public TieredAssignmentSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredAssignmentSnapshot? Find(string code)
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

    public void Save(TieredAssignmentSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredAssignmentSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered assignment snapshots. Empty fields match all.</summary>
public sealed record TieredAssignmentSnapshotQuery(string Department, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(TieredAssignmentSnapshot item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredAssignmentSnapshot> Apply(List<TieredAssignmentSnapshot> items)
    {
        var matched = Collections.Filter(items, (TieredAssignmentSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered assignment snapshot store.</summary>
public static class TieredAssignmentSnapshotStore
{
    public static ITieredAssignmentSnapshotStore NewMemoryTieredAssignmentSnapshotStore()
    {
        return new MemoryTieredAssignmentSnapshotStore(
            new Dictionary<string, TieredAssignmentSnapshot>(),
            new List<string>());
    }

    public static ITieredAssignmentSnapshotStore NewCachedTieredAssignmentSnapshotStore(MemoryTieredAssignmentSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedTieredAssignmentSnapshotStore(
            inner,
            TieredAssignmentSnapshotCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<TieredAssignmentSnapshot> FindTieredAssignmentSnapshots(ITieredAssignmentSnapshotStore store, TieredAssignmentSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
