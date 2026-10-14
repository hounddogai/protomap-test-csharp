namespace Acme.Workforce.Absence.Balance.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance snapshots by code.</summary>
public interface IBalanceSnapshotStore
{
    BalanceSnapshot? Find(string code);
    void Save(BalanceSnapshot item);
    bool Remove(string code);
    List<BalanceSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalanceSnapshotStore(Dictionary<string, BalanceSnapshot> items, List<string> order) : IBalanceSnapshotStore
{
    public Dictionary<string, BalanceSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalanceSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalanceSnapshot item)
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

    public List<BalanceSnapshot> List()
    {
        var outValue = new List<BalanceSnapshot>();
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
public sealed class CachedBalanceSnapshotStore(MemoryBalanceSnapshotStore inner, BalanceSnapshotCache cache, int hits, int misses) : IBalanceSnapshotStore
{
    public MemoryBalanceSnapshotStore Inner { get; set; } = inner;
    public BalanceSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalanceSnapshot? Find(string code)
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

    public void Save(BalanceSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalanceSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance snapshots. Empty fields match all.</summary>
public sealed record BalanceSnapshotQuery(string Segment, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(BalanceSnapshot item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BalanceSnapshot> Apply(List<BalanceSnapshot> items)
    {
        var matched = Collections.Filter(items, (BalanceSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance snapshot store.</summary>
public static class BalanceSnapshotStore
{
    public static IBalanceSnapshotStore NewMemoryBalanceSnapshotStore()
    {
        return new MemoryBalanceSnapshotStore(new Dictionary<string, BalanceSnapshot>(), new List<string>());
    }

    public static IBalanceSnapshotStore NewCachedBalanceSnapshotStore(MemoryBalanceSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedBalanceSnapshotStore(inner, BalanceSnapshotCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BalanceSnapshot> FindBalanceSnapshots(IBalanceSnapshotStore store, BalanceSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
