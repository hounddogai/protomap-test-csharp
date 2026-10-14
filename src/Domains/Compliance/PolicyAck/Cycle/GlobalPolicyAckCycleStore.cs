namespace Acme.Workforce.Compliance.PolicyAck.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global policy ack cycles by code.</summary>
public interface IGlobalPolicyAckCycleStore
{
    GlobalPolicyAckCycle? Find(string code);
    void Save(GlobalPolicyAckCycle item);
    bool Remove(string code);
    List<GlobalPolicyAckCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalPolicyAckCycleStore(Dictionary<string, GlobalPolicyAckCycle> items, List<string> order) : IGlobalPolicyAckCycleStore
{
    public Dictionary<string, GlobalPolicyAckCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalPolicyAckCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalPolicyAckCycle item)
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

    public List<GlobalPolicyAckCycle> List()
    {
        var outValue = new List<GlobalPolicyAckCycle>();
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
public sealed class CachedGlobalPolicyAckCycleStore(MemoryGlobalPolicyAckCycleStore inner, GlobalPolicyAckCycleCache cache, int hits, int misses) : IGlobalPolicyAckCycleStore
{
    public MemoryGlobalPolicyAckCycleStore Inner { get; set; } = inner;
    public GlobalPolicyAckCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalPolicyAckCycle? Find(string code)
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

    public void Save(GlobalPolicyAckCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalPolicyAckCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global policy ack cycles. Empty fields match all.</summary>
public sealed record GlobalPolicyAckCycleQuery(string Title, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalPolicyAckCycle item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalPolicyAckCycle> Apply(List<GlobalPolicyAckCycle> items)
    {
        var matched = Collections.Filter(items, (GlobalPolicyAckCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global policy ack cycle store.</summary>
public static class GlobalPolicyAckCycleStore
{
    public static IGlobalPolicyAckCycleStore NewMemoryGlobalPolicyAckCycleStore()
    {
        return new MemoryGlobalPolicyAckCycleStore(
            new Dictionary<string, GlobalPolicyAckCycle>(),
            new List<string>());
    }

    public static IGlobalPolicyAckCycleStore NewCachedGlobalPolicyAckCycleStore(MemoryGlobalPolicyAckCycleStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalPolicyAckCycleStore(inner, GlobalPolicyAckCycleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<GlobalPolicyAckCycle> FindGlobalPolicyAckCycles(IGlobalPolicyAckCycleStore store, GlobalPolicyAckCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
