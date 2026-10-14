namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient policy ack cycles by code.</summary>
public interface ILenientPolicyAckCycleStore
{
    LenientPolicyAckCycle? Find(string code);
    void Save(LenientPolicyAckCycle item);
    bool Remove(string code);
    List<LenientPolicyAckCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientPolicyAckCycleStore(Dictionary<string, LenientPolicyAckCycle> items, List<string> order) : ILenientPolicyAckCycleStore
{
    public Dictionary<string, LenientPolicyAckCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientPolicyAckCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientPolicyAckCycle item)
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

    public List<LenientPolicyAckCycle> List()
    {
        var outValue = new List<LenientPolicyAckCycle>();
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
public sealed class CachedLenientPolicyAckCycleStore(MemoryLenientPolicyAckCycleStore inner, LenientPolicyAckCycleCache cache, int hits, int misses) : ILenientPolicyAckCycleStore
{
    public MemoryLenientPolicyAckCycleStore Inner { get; set; } = inner;
    public LenientPolicyAckCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientPolicyAckCycle? Find(string code)
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

    public void Save(LenientPolicyAckCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientPolicyAckCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient policy ack cycles. Empty fields match all.</summary>
public sealed record LenientPolicyAckCycleQuery(string Label, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(LenientPolicyAckCycle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientPolicyAckCycle> Apply(List<LenientPolicyAckCycle> items)
    {
        var matched = Collections.Filter(items, (LenientPolicyAckCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient policy ack cycle store.</summary>
public static class LenientPolicyAckCycleStore
{
    public static ILenientPolicyAckCycleStore NewMemoryLenientPolicyAckCycleStore()
    {
        return new MemoryLenientPolicyAckCycleStore(
            new Dictionary<string, LenientPolicyAckCycle>(),
            new List<string>());
    }

    public static ILenientPolicyAckCycleStore NewCachedLenientPolicyAckCycleStore(MemoryLenientPolicyAckCycleStore inner, int ttl, IClock clock)
    {
        return new CachedLenientPolicyAckCycleStore(inner, LenientPolicyAckCycleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LenientPolicyAckCycle> FindLenientPolicyAckCycles(ILenientPolicyAckCycleStore store, LenientPolicyAckCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
