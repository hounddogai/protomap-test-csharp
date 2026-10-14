namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge snapshots by code.</summary>
public interface IAccessBadgeSnapshotStore
{
    AccessBadgeSnapshot? Find(string code);
    void Save(AccessBadgeSnapshot item);
    bool Remove(string code);
    List<AccessBadgeSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeSnapshotStore(Dictionary<string, AccessBadgeSnapshot> items, List<string> order) : IAccessBadgeSnapshotStore
{
    public Dictionary<string, AccessBadgeSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeSnapshot item)
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

    public List<AccessBadgeSnapshot> List()
    {
        var outValue = new List<AccessBadgeSnapshot>();
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
public sealed class CachedAccessBadgeSnapshotStore(MemoryAccessBadgeSnapshotStore inner, AccessBadgeSnapshotCache cache, int hits, int misses) : IAccessBadgeSnapshotStore
{
    public MemoryAccessBadgeSnapshotStore Inner { get; set; } = inner;
    public AccessBadgeSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeSnapshot? Find(string code)
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

    public void Save(AccessBadgeSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge snapshots. Empty fields match all.</summary>
public sealed record AccessBadgeSnapshotQuery(string Label, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeSnapshot item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccessBadgeSnapshot> Apply(List<AccessBadgeSnapshot> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge snapshot store.</summary>
public static class AccessBadgeSnapshotStore
{
    public static IAccessBadgeSnapshotStore NewMemoryAccessBadgeSnapshotStore()
    {
        return new MemoryAccessBadgeSnapshotStore(new Dictionary<string, AccessBadgeSnapshot>(), new List<string>());
    }

    public static IAccessBadgeSnapshotStore NewCachedAccessBadgeSnapshotStore(MemoryAccessBadgeSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeSnapshotStore(inner, AccessBadgeSnapshotCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeSnapshot> FindAccessBadgeSnapshots(IAccessBadgeSnapshotStore store, AccessBadgeSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
