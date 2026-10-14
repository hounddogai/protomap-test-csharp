namespace Acme.Workforce.Absence.Holiday.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending holiday snapshots by code.</summary>
public interface IPendingHolidaySnapshotStore
{
    PendingHolidaySnapshot? Find(string code);
    void Save(PendingHolidaySnapshot item);
    bool Remove(string code);
    List<PendingHolidaySnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingHolidaySnapshotStore(Dictionary<string, PendingHolidaySnapshot> items, List<string> order) : IPendingHolidaySnapshotStore
{
    public Dictionary<string, PendingHolidaySnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingHolidaySnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingHolidaySnapshot item)
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

    public List<PendingHolidaySnapshot> List()
    {
        var outValue = new List<PendingHolidaySnapshot>();
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
public sealed class CachedPendingHolidaySnapshotStore(MemoryPendingHolidaySnapshotStore inner, PendingHolidaySnapshotCache cache, int hits, int misses) : IPendingHolidaySnapshotStore
{
    public MemoryPendingHolidaySnapshotStore Inner { get; set; } = inner;
    public PendingHolidaySnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingHolidaySnapshot? Find(string code)
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

    public void Save(PendingHolidaySnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingHolidaySnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending holiday snapshots. Empty fields match all.</summary>
public sealed record PendingHolidaySnapshotQuery(string Name, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PendingHolidaySnapshot item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingHolidaySnapshot> Apply(List<PendingHolidaySnapshot> items)
    {
        var matched = Collections.Filter(items, (PendingHolidaySnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending holiday snapshot store.</summary>
public static class PendingHolidaySnapshotStore
{
    public static IPendingHolidaySnapshotStore NewMemoryPendingHolidaySnapshotStore()
    {
        return new MemoryPendingHolidaySnapshotStore(
            new Dictionary<string, PendingHolidaySnapshot>(),
            new List<string>());
    }

    public static IPendingHolidaySnapshotStore NewCachedPendingHolidaySnapshotStore(MemoryPendingHolidaySnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedPendingHolidaySnapshotStore(
            inner,
            PendingHolidaySnapshotCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PendingHolidaySnapshot> FindPendingHolidaySnapshots(IPendingHolidaySnapshotStore store, PendingHolidaySnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
