namespace Acme.Workforce.Compliance.PolicyAck.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback policy ack snapshots by code.</summary>
public interface IFallbackPolicyAckSnapshotStore
{
    FallbackPolicyAckSnapshot? Find(string code);
    void Save(FallbackPolicyAckSnapshot item);
    bool Remove(string code);
    List<FallbackPolicyAckSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackPolicyAckSnapshotStore(Dictionary<string, FallbackPolicyAckSnapshot> items, List<string> order) : IFallbackPolicyAckSnapshotStore
{
    public Dictionary<string, FallbackPolicyAckSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackPolicyAckSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackPolicyAckSnapshot item)
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

    public List<FallbackPolicyAckSnapshot> List()
    {
        var outValue = new List<FallbackPolicyAckSnapshot>();
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
public sealed class CachedFallbackPolicyAckSnapshotStore(MemoryFallbackPolicyAckSnapshotStore inner, FallbackPolicyAckSnapshotCache cache, int hits, int misses) : IFallbackPolicyAckSnapshotStore
{
    public MemoryFallbackPolicyAckSnapshotStore Inner { get; set; } = inner;
    public FallbackPolicyAckSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackPolicyAckSnapshot? Find(string code)
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

    public void Save(FallbackPolicyAckSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackPolicyAckSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback policy ack snapshots. Empty fields match all.</summary>
public sealed record FallbackPolicyAckSnapshotQuery(string Name, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackPolicyAckSnapshot item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackPolicyAckSnapshot> Apply(List<FallbackPolicyAckSnapshot> items)
    {
        var matched = Collections.Filter(items, (FallbackPolicyAckSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback policy ack snapshot store.</summary>
public static class FallbackPolicyAckSnapshotStore
{
    public static IFallbackPolicyAckSnapshotStore NewMemoryFallbackPolicyAckSnapshotStore()
    {
        return new MemoryFallbackPolicyAckSnapshotStore(
            new Dictionary<string, FallbackPolicyAckSnapshot>(),
            new List<string>());
    }

    public static IFallbackPolicyAckSnapshotStore NewCachedFallbackPolicyAckSnapshotStore(MemoryFallbackPolicyAckSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackPolicyAckSnapshotStore(
            inner,
            FallbackPolicyAckSnapshotCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<FallbackPolicyAckSnapshot> FindFallbackPolicyAckSnapshots(IFallbackPolicyAckSnapshotStore store, FallbackPolicyAckSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
