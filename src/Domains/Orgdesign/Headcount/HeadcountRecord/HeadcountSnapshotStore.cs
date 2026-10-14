namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount snapshots by code.</summary>
public interface IHeadcountSnapshotStore
{
    HeadcountSnapshot? Find(string code);
    void Save(HeadcountSnapshot item);
    bool Remove(string code);
    List<HeadcountSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountSnapshotStore(Dictionary<string, HeadcountSnapshot> items, List<string> order) : IHeadcountSnapshotStore
{
    public Dictionary<string, HeadcountSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountSnapshot item)
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

    public List<HeadcountSnapshot> List()
    {
        var outValue = new List<HeadcountSnapshot>();
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
public sealed class CachedHeadcountSnapshotStore(MemoryHeadcountSnapshotStore inner, HeadcountSnapshotCache cache, int hits, int misses) : IHeadcountSnapshotStore
{
    public MemoryHeadcountSnapshotStore Inner { get; set; } = inner;
    public HeadcountSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountSnapshot? Find(string code)
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

    public void Save(HeadcountSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount snapshots. Empty fields match all.</summary>
public sealed record HeadcountSnapshotQuery(string Unit, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountSnapshot item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountSnapshot> Apply(List<HeadcountSnapshot> items)
    {
        var matched = Collections.Filter(items, (HeadcountSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount snapshot store.</summary>
public static class HeadcountSnapshotStore
{
    public static IHeadcountSnapshotStore NewMemoryHeadcountSnapshotStore()
    {
        return new MemoryHeadcountSnapshotStore(new Dictionary<string, HeadcountSnapshot>(), new List<string>());
    }

    public static IHeadcountSnapshotStore NewCachedHeadcountSnapshotStore(MemoryHeadcountSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountSnapshotStore(inner, HeadcountSnapshotCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<HeadcountSnapshot> FindHeadcountSnapshots(IHeadcountSnapshotStore store, HeadcountSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
