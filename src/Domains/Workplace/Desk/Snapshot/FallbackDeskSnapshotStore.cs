namespace Acme.Workforce.Workplace.Desk.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback desk snapshots by code.</summary>
public interface IFallbackDeskSnapshotStore
{
    FallbackDeskSnapshot? Find(string code);
    void Save(FallbackDeskSnapshot item);
    bool Remove(string code);
    List<FallbackDeskSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackDeskSnapshotStore(Dictionary<string, FallbackDeskSnapshot> items, List<string> order) : IFallbackDeskSnapshotStore
{
    public Dictionary<string, FallbackDeskSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackDeskSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackDeskSnapshot item)
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

    public List<FallbackDeskSnapshot> List()
    {
        var outValue = new List<FallbackDeskSnapshot>();
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
public sealed class CachedFallbackDeskSnapshotStore(MemoryFallbackDeskSnapshotStore inner, FallbackDeskSnapshotCache cache, int hits, int misses) : IFallbackDeskSnapshotStore
{
    public MemoryFallbackDeskSnapshotStore Inner { get; set; } = inner;
    public FallbackDeskSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackDeskSnapshot? Find(string code)
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

    public void Save(FallbackDeskSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackDeskSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback desk snapshots. Empty fields match all.</summary>
public sealed record FallbackDeskSnapshotQuery(string Title, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackDeskSnapshot item)
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

    public List<FallbackDeskSnapshot> Apply(List<FallbackDeskSnapshot> items)
    {
        var matched = Collections.Filter(items, (FallbackDeskSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback desk snapshot store.</summary>
public static class FallbackDeskSnapshotStore
{
    public static IFallbackDeskSnapshotStore NewMemoryFallbackDeskSnapshotStore()
    {
        return new MemoryFallbackDeskSnapshotStore(
            new Dictionary<string, FallbackDeskSnapshot>(),
            new List<string>());
    }

    public static IFallbackDeskSnapshotStore NewCachedFallbackDeskSnapshotStore(MemoryFallbackDeskSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackDeskSnapshotStore(inner, FallbackDeskSnapshotCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<FallbackDeskSnapshot> FindFallbackDeskSnapshots(IFallbackDeskSnapshotStore store, FallbackDeskSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
