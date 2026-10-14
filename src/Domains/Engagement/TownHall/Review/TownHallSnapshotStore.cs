namespace Acme.Workforce.Engagement.TownHall.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall snapshots by code.</summary>
public interface ITownHallSnapshotStore
{
    TownHallSnapshot? Find(string code);
    void Save(TownHallSnapshot item);
    bool Remove(string code);
    List<TownHallSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallSnapshotStore(Dictionary<string, TownHallSnapshot> items, List<string> order) : ITownHallSnapshotStore
{
    public Dictionary<string, TownHallSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallSnapshot item)
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

    public List<TownHallSnapshot> List()
    {
        var outValue = new List<TownHallSnapshot>();
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
public sealed class CachedTownHallSnapshotStore(MemoryTownHallSnapshotStore inner, TownHallSnapshotCache cache, int hits, int misses) : ITownHallSnapshotStore
{
    public MemoryTownHallSnapshotStore Inner { get; set; } = inner;
    public TownHallSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallSnapshot? Find(string code)
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

    public void Save(TownHallSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall snapshots. Empty fields match all.</summary>
public sealed record TownHallSnapshotQuery(string Label, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallSnapshot item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TownHallSnapshot> Apply(List<TownHallSnapshot> items)
    {
        var matched = Collections.Filter(items, (TownHallSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall snapshot store.</summary>
public static class TownHallSnapshotStore
{
    public static ITownHallSnapshotStore NewMemoryTownHallSnapshotStore()
    {
        return new MemoryTownHallSnapshotStore(new Dictionary<string, TownHallSnapshot>(), new List<string>());
    }

    public static ITownHallSnapshotStore NewCachedTownHallSnapshotStore(MemoryTownHallSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallSnapshotStore(inner, TownHallSnapshotCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TownHallSnapshot> FindTownHallSnapshots(ITownHallSnapshotStore store, TownHallSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
