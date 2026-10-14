namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict garnishment snapshots by code.</summary>
public interface IStrictGarnishmentSnapshotStore
{
    StrictGarnishmentSnapshot? Find(string code);
    void Save(StrictGarnishmentSnapshot item);
    bool Remove(string code);
    List<StrictGarnishmentSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictGarnishmentSnapshotStore(Dictionary<string, StrictGarnishmentSnapshot> items, List<string> order) : IStrictGarnishmentSnapshotStore
{
    public Dictionary<string, StrictGarnishmentSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictGarnishmentSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictGarnishmentSnapshot item)
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

    public List<StrictGarnishmentSnapshot> List()
    {
        var outValue = new List<StrictGarnishmentSnapshot>();
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
public sealed class CachedStrictGarnishmentSnapshotStore(MemoryStrictGarnishmentSnapshotStore inner, StrictGarnishmentSnapshotCache cache, int hits, int misses) : IStrictGarnishmentSnapshotStore
{
    public MemoryStrictGarnishmentSnapshotStore Inner { get; set; } = inner;
    public StrictGarnishmentSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictGarnishmentSnapshot? Find(string code)
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

    public void Save(StrictGarnishmentSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictGarnishmentSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict garnishment snapshots. Empty fields match all.</summary>
public sealed record StrictGarnishmentSnapshotQuery(string Region, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(StrictGarnishmentSnapshot item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<StrictGarnishmentSnapshot> Apply(List<StrictGarnishmentSnapshot> items)
    {
        var matched = Collections.Filter(items, (StrictGarnishmentSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict garnishment snapshot store.</summary>
public static class StrictGarnishmentSnapshotStore
{
    public static IStrictGarnishmentSnapshotStore NewMemoryStrictGarnishmentSnapshotStore()
    {
        return new MemoryStrictGarnishmentSnapshotStore(
            new Dictionary<string, StrictGarnishmentSnapshot>(),
            new List<string>());
    }

    public static IStrictGarnishmentSnapshotStore NewCachedStrictGarnishmentSnapshotStore(MemoryStrictGarnishmentSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedStrictGarnishmentSnapshotStore(
            inner,
            StrictGarnishmentSnapshotCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<StrictGarnishmentSnapshot> FindStrictGarnishmentSnapshots(IStrictGarnishmentSnapshotStore store, StrictGarnishmentSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
