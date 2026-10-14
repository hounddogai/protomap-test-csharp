namespace Acme.Workforce.Orgdesign.Position.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position snapshots by code.</summary>
public interface IPositionSnapshotStore
{
    PositionSnapshot? Find(string code);
    void Save(PositionSnapshot item);
    bool Remove(string code);
    List<PositionSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionSnapshotStore(Dictionary<string, PositionSnapshot> items, List<string> order) : IPositionSnapshotStore
{
    public Dictionary<string, PositionSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionSnapshot item)
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

    public List<PositionSnapshot> List()
    {
        var outValue = new List<PositionSnapshot>();
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
public sealed class CachedPositionSnapshotStore(MemoryPositionSnapshotStore inner, PositionSnapshotCache cache, int hits, int misses) : IPositionSnapshotStore
{
    public MemoryPositionSnapshotStore Inner { get; set; } = inner;
    public PositionSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionSnapshot? Find(string code)
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

    public void Save(PositionSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position snapshots. Empty fields match all.</summary>
public sealed record PositionSnapshotQuery(string Label, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(PositionSnapshot item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PositionSnapshot> Apply(List<PositionSnapshot> items)
    {
        var matched = Collections.Filter(items, (PositionSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position snapshot store.</summary>
public static class PositionSnapshotStore
{
    public static IPositionSnapshotStore NewMemoryPositionSnapshotStore()
    {
        return new MemoryPositionSnapshotStore(new Dictionary<string, PositionSnapshot>(), new List<string>());
    }

    public static IPositionSnapshotStore NewCachedPositionSnapshotStore(MemoryPositionSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedPositionSnapshotStore(inner, PositionSnapshotCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PositionSnapshot> FindPositionSnapshots(IPositionSnapshotStore store, PositionSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
