namespace Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback holiday snapshots by code.</summary>
public interface IFallbackHolidaySnapshotStore
{
    FallbackHolidaySnapshot? Find(string code);
    void Save(FallbackHolidaySnapshot item);
    bool Remove(string code);
    List<FallbackHolidaySnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackHolidaySnapshotStore(Dictionary<string, FallbackHolidaySnapshot> items, List<string> order) : IFallbackHolidaySnapshotStore
{
    public Dictionary<string, FallbackHolidaySnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackHolidaySnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackHolidaySnapshot item)
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

    public List<FallbackHolidaySnapshot> List()
    {
        var outValue = new List<FallbackHolidaySnapshot>();
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
public sealed class CachedFallbackHolidaySnapshotStore(MemoryFallbackHolidaySnapshotStore inner, FallbackHolidaySnapshotCache cache, int hits, int misses) : IFallbackHolidaySnapshotStore
{
    public MemoryFallbackHolidaySnapshotStore Inner { get; set; } = inner;
    public FallbackHolidaySnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackHolidaySnapshot? Find(string code)
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

    public void Save(FallbackHolidaySnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackHolidaySnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback holiday snapshots. Empty fields match all.</summary>
public sealed record FallbackHolidaySnapshotQuery(string Label, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackHolidaySnapshot item)
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

    public List<FallbackHolidaySnapshot> Apply(List<FallbackHolidaySnapshot> items)
    {
        var matched = Collections.Filter(items, (FallbackHolidaySnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback holiday snapshot store.</summary>
public static class FallbackHolidaySnapshotStore
{
    public static IFallbackHolidaySnapshotStore NewMemoryFallbackHolidaySnapshotStore()
    {
        return new MemoryFallbackHolidaySnapshotStore(
            new Dictionary<string, FallbackHolidaySnapshot>(),
            new List<string>());
    }

    public static IFallbackHolidaySnapshotStore NewCachedFallbackHolidaySnapshotStore(MemoryFallbackHolidaySnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackHolidaySnapshotStore(
            inner,
            FallbackHolidaySnapshotCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<FallbackHolidaySnapshot> FindFallbackHolidaySnapshots(IFallbackHolidaySnapshotStore store, FallbackHolidaySnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
