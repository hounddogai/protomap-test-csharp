namespace Acme.Workforce.Payrollops.Garnishment.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment snapshots by code.</summary>
public interface IGarnishmentSnapshotStore
{
    GarnishmentSnapshot? Find(string code);
    void Save(GarnishmentSnapshot item);
    bool Remove(string code);
    List<GarnishmentSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentSnapshotStore(Dictionary<string, GarnishmentSnapshot> items, List<string> order) : IGarnishmentSnapshotStore
{
    public Dictionary<string, GarnishmentSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentSnapshot item)
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

    public List<GarnishmentSnapshot> List()
    {
        var outValue = new List<GarnishmentSnapshot>();
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
public sealed class CachedGarnishmentSnapshotStore(MemoryGarnishmentSnapshotStore inner, GarnishmentSnapshotCache cache, int hits, int misses) : IGarnishmentSnapshotStore
{
    public MemoryGarnishmentSnapshotStore Inner { get; set; } = inner;
    public GarnishmentSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentSnapshot? Find(string code)
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

    public void Save(GarnishmentSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment snapshots. Empty fields match all.</summary>
public sealed record GarnishmentSnapshotQuery(string Region, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentSnapshot item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GarnishmentSnapshot> Apply(List<GarnishmentSnapshot> items)
    {
        var matched = Collections.Filter(items, (GarnishmentSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment snapshot store.</summary>
public static class GarnishmentSnapshotStore
{
    public static IGarnishmentSnapshotStore NewMemoryGarnishmentSnapshotStore()
    {
        return new MemoryGarnishmentSnapshotStore(new Dictionary<string, GarnishmentSnapshot>(), new List<string>());
    }

    public static IGarnishmentSnapshotStore NewCachedGarnishmentSnapshotStore(MemoryGarnishmentSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentSnapshotStore(inner, GarnishmentSnapshotCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<GarnishmentSnapshot> FindGarnishmentSnapshots(IGarnishmentSnapshotStore store, GarnishmentSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
