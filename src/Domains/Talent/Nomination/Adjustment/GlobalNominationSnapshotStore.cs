namespace Acme.Workforce.Talent.Nomination.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global nomination snapshots by code.</summary>
public interface IGlobalNominationSnapshotStore
{
    GlobalNominationSnapshot? Find(string code);
    void Save(GlobalNominationSnapshot item);
    bool Remove(string code);
    List<GlobalNominationSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalNominationSnapshotStore(Dictionary<string, GlobalNominationSnapshot> items, List<string> order) : IGlobalNominationSnapshotStore
{
    public Dictionary<string, GlobalNominationSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalNominationSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalNominationSnapshot item)
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

    public List<GlobalNominationSnapshot> List()
    {
        var outValue = new List<GlobalNominationSnapshot>();
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
public sealed class CachedGlobalNominationSnapshotStore(MemoryGlobalNominationSnapshotStore inner, GlobalNominationSnapshotCache cache, int hits, int misses) : IGlobalNominationSnapshotStore
{
    public MemoryGlobalNominationSnapshotStore Inner { get; set; } = inner;
    public GlobalNominationSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalNominationSnapshot? Find(string code)
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

    public void Save(GlobalNominationSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalNominationSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global nomination snapshots. Empty fields match all.</summary>
public sealed record GlobalNominationSnapshotQuery(string Title, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalNominationSnapshot item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalNominationSnapshot> Apply(List<GlobalNominationSnapshot> items)
    {
        var matched = Collections.Filter(items, (GlobalNominationSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global nomination snapshot store.</summary>
public static class GlobalNominationSnapshotStore
{
    public static IGlobalNominationSnapshotStore NewMemoryGlobalNominationSnapshotStore()
    {
        return new MemoryGlobalNominationSnapshotStore(
            new Dictionary<string, GlobalNominationSnapshot>(),
            new List<string>());
    }

    public static IGlobalNominationSnapshotStore NewCachedGlobalNominationSnapshotStore(MemoryGlobalNominationSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalNominationSnapshotStore(
            inner,
            GlobalNominationSnapshotCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<GlobalNominationSnapshot> FindGlobalNominationSnapshots(IGlobalNominationSnapshotStore store, GlobalNominationSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
