namespace Acme.Workforce.Compliance.PolicyAck.AckGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict policy ack groups by code.</summary>
public interface IStrictPolicyAckGroupStore
{
    StrictPolicyAckGroup? Find(string code);
    void Save(StrictPolicyAckGroup item);
    bool Remove(string code);
    List<StrictPolicyAckGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictPolicyAckGroupStore(Dictionary<string, StrictPolicyAckGroup> items, List<string> order) : IStrictPolicyAckGroupStore
{
    public Dictionary<string, StrictPolicyAckGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictPolicyAckGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictPolicyAckGroup item)
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

    public List<StrictPolicyAckGroup> List()
    {
        var outValue = new List<StrictPolicyAckGroup>();
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
public sealed class CachedStrictPolicyAckGroupStore(MemoryStrictPolicyAckGroupStore inner, StrictPolicyAckGroupCache cache, int hits, int misses) : IStrictPolicyAckGroupStore
{
    public MemoryStrictPolicyAckGroupStore Inner { get; set; } = inner;
    public StrictPolicyAckGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictPolicyAckGroup? Find(string code)
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

    public void Save(StrictPolicyAckGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictPolicyAckGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict policy ack groups. Empty fields match all.</summary>
public sealed record StrictPolicyAckGroupQuery(string Department, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(StrictPolicyAckGroup item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictPolicyAckGroup> Apply(List<StrictPolicyAckGroup> items)
    {
        var matched = Collections.Filter(items, (StrictPolicyAckGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict policy ack group store.</summary>
public static class StrictPolicyAckGroupStore
{
    public static IStrictPolicyAckGroupStore NewMemoryStrictPolicyAckGroupStore()
    {
        return new MemoryStrictPolicyAckGroupStore(
            new Dictionary<string, StrictPolicyAckGroup>(),
            new List<string>());
    }

    public static IStrictPolicyAckGroupStore NewCachedStrictPolicyAckGroupStore(MemoryStrictPolicyAckGroupStore inner, int ttl, IClock clock)
    {
        return new CachedStrictPolicyAckGroupStore(inner, StrictPolicyAckGroupCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<StrictPolicyAckGroup> FindStrictPolicyAckGroups(IStrictPolicyAckGroupStore store, StrictPolicyAckGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
