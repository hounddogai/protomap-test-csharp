namespace Acme.Workforce.Assets.Assignment.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment snapshots by code.</summary>
public interface IAssignmentSnapshotStore
{
    AssignmentSnapshot? Find(string code);
    void Save(AssignmentSnapshot item);
    bool Remove(string code);
    List<AssignmentSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentSnapshotStore(Dictionary<string, AssignmentSnapshot> items, List<string> order) : IAssignmentSnapshotStore
{
    public Dictionary<string, AssignmentSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentSnapshot item)
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

    public List<AssignmentSnapshot> List()
    {
        var outValue = new List<AssignmentSnapshot>();
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
public sealed class CachedAssignmentSnapshotStore(MemoryAssignmentSnapshotStore inner, AssignmentSnapshotCache cache, int hits, int misses) : IAssignmentSnapshotStore
{
    public MemoryAssignmentSnapshotStore Inner { get; set; } = inner;
    public AssignmentSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentSnapshot? Find(string code)
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

    public void Save(AssignmentSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment snapshots. Empty fields match all.</summary>
public sealed record AssignmentSnapshotQuery(string Name, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentSnapshot item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentSnapshot> Apply(List<AssignmentSnapshot> items)
    {
        var matched = Collections.Filter(items, (AssignmentSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment snapshot store.</summary>
public static class AssignmentSnapshotStore
{
    public static IAssignmentSnapshotStore NewMemoryAssignmentSnapshotStore()
    {
        return new MemoryAssignmentSnapshotStore(new Dictionary<string, AssignmentSnapshot>(), new List<string>());
    }

    public static IAssignmentSnapshotStore NewCachedAssignmentSnapshotStore(MemoryAssignmentSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentSnapshotStore(inner, AssignmentSnapshotCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AssignmentSnapshot> FindAssignmentSnapshots(IAssignmentSnapshotStore store, AssignmentSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
