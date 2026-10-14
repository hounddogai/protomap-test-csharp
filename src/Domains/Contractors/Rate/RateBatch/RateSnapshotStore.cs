namespace Acme.Workforce.Contractors.Rate.RateBatch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate snapshots by code.</summary>
public interface IRateSnapshotStore
{
    RateSnapshot? Find(string code);
    void Save(RateSnapshot item);
    bool Remove(string code);
    List<RateSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateSnapshotStore(Dictionary<string, RateSnapshot> items, List<string> order) : IRateSnapshotStore
{
    public Dictionary<string, RateSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateSnapshot item)
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

    public List<RateSnapshot> List()
    {
        var outValue = new List<RateSnapshot>();
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
public sealed class CachedRateSnapshotStore(MemoryRateSnapshotStore inner, RateSnapshotCache cache, int hits, int misses) : IRateSnapshotStore
{
    public MemoryRateSnapshotStore Inner { get; set; } = inner;
    public RateSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateSnapshot? Find(string code)
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

    public void Save(RateSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate snapshots. Empty fields match all.</summary>
public sealed record RateSnapshotQuery(string Region, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(RateSnapshot item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RateSnapshot> Apply(List<RateSnapshot> items)
    {
        var matched = Collections.Filter(items, (RateSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate snapshot store.</summary>
public static class RateSnapshotStore
{
    public static IRateSnapshotStore NewMemoryRateSnapshotStore()
    {
        return new MemoryRateSnapshotStore(new Dictionary<string, RateSnapshot>(), new List<string>());
    }

    public static IRateSnapshotStore NewCachedRateSnapshotStore(MemoryRateSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedRateSnapshotStore(inner, RateSnapshotCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RateSnapshot> FindRateSnapshots(IRateSnapshotStore store, RateSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
