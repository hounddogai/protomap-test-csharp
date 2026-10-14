namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty snapshots by code.</summary>
public interface IWarrantySnapshotStore
{
    WarrantySnapshot? Find(string code);
    void Save(WarrantySnapshot item);
    bool Remove(string code);
    List<WarrantySnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantySnapshotStore(Dictionary<string, WarrantySnapshot> items, List<string> order) : IWarrantySnapshotStore
{
    public Dictionary<string, WarrantySnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantySnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantySnapshot item)
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

    public List<WarrantySnapshot> List()
    {
        var outValue = new List<WarrantySnapshot>();
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
public sealed class CachedWarrantySnapshotStore(MemoryWarrantySnapshotStore inner, WarrantySnapshotCache cache, int hits, int misses) : IWarrantySnapshotStore
{
    public MemoryWarrantySnapshotStore Inner { get; set; } = inner;
    public WarrantySnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantySnapshot? Find(string code)
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

    public void Save(WarrantySnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantySnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty snapshots. Empty fields match all.</summary>
public sealed record WarrantySnapshotQuery(string Label, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantySnapshot item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantySnapshot> Apply(List<WarrantySnapshot> items)
    {
        var matched = Collections.Filter(items, (WarrantySnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty snapshot store.</summary>
public static class WarrantySnapshotStore
{
    public static IWarrantySnapshotStore NewMemoryWarrantySnapshotStore()
    {
        return new MemoryWarrantySnapshotStore(new Dictionary<string, WarrantySnapshot>(), new List<string>());
    }

    public static IWarrantySnapshotStore NewCachedWarrantySnapshotStore(MemoryWarrantySnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantySnapshotStore(inner, WarrantySnapshotCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<WarrantySnapshot> FindWarrantySnapshots(IWarrantySnapshotStore store, WarrantySnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
