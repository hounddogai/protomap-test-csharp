namespace Acme.Workforce.Compensation.EquityGrant.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant snapshots by code.</summary>
public interface IEquityGrantSnapshotStore
{
    EquityGrantSnapshot? Find(string code);
    void Save(EquityGrantSnapshot item);
    bool Remove(string code);
    List<EquityGrantSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantSnapshotStore(Dictionary<string, EquityGrantSnapshot> items, List<string> order) : IEquityGrantSnapshotStore
{
    public Dictionary<string, EquityGrantSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantSnapshot item)
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

    public List<EquityGrantSnapshot> List()
    {
        var outValue = new List<EquityGrantSnapshot>();
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
public sealed class CachedEquityGrantSnapshotStore(MemoryEquityGrantSnapshotStore inner, EquityGrantSnapshotCache cache, int hits, int misses) : IEquityGrantSnapshotStore
{
    public MemoryEquityGrantSnapshotStore Inner { get; set; } = inner;
    public EquityGrantSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantSnapshot? Find(string code)
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

    public void Save(EquityGrantSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant snapshots. Empty fields match all.</summary>
public sealed record EquityGrantSnapshotQuery(string Label, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantSnapshot item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<EquityGrantSnapshot> Apply(List<EquityGrantSnapshot> items)
    {
        var matched = Collections.Filter(items, (EquityGrantSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant snapshot store.</summary>
public static class EquityGrantSnapshotStore
{
    public static IEquityGrantSnapshotStore NewMemoryEquityGrantSnapshotStore()
    {
        return new MemoryEquityGrantSnapshotStore(new Dictionary<string, EquityGrantSnapshot>(), new List<string>());
    }

    public static IEquityGrantSnapshotStore NewCachedEquityGrantSnapshotStore(MemoryEquityGrantSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantSnapshotStore(inner, EquityGrantSnapshotCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<EquityGrantSnapshot> FindEquityGrantSnapshots(IEquityGrantSnapshotStore store, EquityGrantSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
