namespace Acme.Workforce.Compensation.PayBand.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback pay band snapshots by code.</summary>
public interface IFallbackPayBandSnapshotStore
{
    FallbackPayBandSnapshot? Find(string code);
    void Save(FallbackPayBandSnapshot item);
    bool Remove(string code);
    List<FallbackPayBandSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackPayBandSnapshotStore(Dictionary<string, FallbackPayBandSnapshot> items, List<string> order) : IFallbackPayBandSnapshotStore
{
    public Dictionary<string, FallbackPayBandSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackPayBandSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackPayBandSnapshot item)
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

    public List<FallbackPayBandSnapshot> List()
    {
        var outValue = new List<FallbackPayBandSnapshot>();
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
public sealed class CachedFallbackPayBandSnapshotStore(MemoryFallbackPayBandSnapshotStore inner, FallbackPayBandSnapshotCache cache, int hits, int misses) : IFallbackPayBandSnapshotStore
{
    public MemoryFallbackPayBandSnapshotStore Inner { get; set; } = inner;
    public FallbackPayBandSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackPayBandSnapshot? Find(string code)
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

    public void Save(FallbackPayBandSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackPayBandSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback pay band snapshots. Empty fields match all.</summary>
public sealed record FallbackPayBandSnapshotQuery(string Title, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackPayBandSnapshot item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackPayBandSnapshot> Apply(List<FallbackPayBandSnapshot> items)
    {
        var matched = Collections.Filter(items, (FallbackPayBandSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback pay band snapshot store.</summary>
public static class FallbackPayBandSnapshotStore
{
    public static IFallbackPayBandSnapshotStore NewMemoryFallbackPayBandSnapshotStore()
    {
        return new MemoryFallbackPayBandSnapshotStore(
            new Dictionary<string, FallbackPayBandSnapshot>(),
            new List<string>());
    }

    public static IFallbackPayBandSnapshotStore NewCachedFallbackPayBandSnapshotStore(MemoryFallbackPayBandSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackPayBandSnapshotStore(
            inner,
            FallbackPayBandSnapshotCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<FallbackPayBandSnapshot> FindFallbackPayBandSnapshots(IFallbackPayBandSnapshotStore store, FallbackPayBandSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
