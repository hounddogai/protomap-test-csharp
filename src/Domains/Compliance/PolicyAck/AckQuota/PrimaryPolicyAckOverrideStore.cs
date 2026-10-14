namespace Acme.Workforce.Compliance.PolicyAck.AckQuota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary policy ack overrides by code.</summary>
public interface IPrimaryPolicyAckOverrideStore
{
    PrimaryPolicyAckOverride? Find(string code);
    void Save(PrimaryPolicyAckOverride item);
    bool Remove(string code);
    List<PrimaryPolicyAckOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryPolicyAckOverrideStore(Dictionary<string, PrimaryPolicyAckOverride> items, List<string> order) : IPrimaryPolicyAckOverrideStore
{
    public Dictionary<string, PrimaryPolicyAckOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryPolicyAckOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryPolicyAckOverride item)
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

    public List<PrimaryPolicyAckOverride> List()
    {
        var outValue = new List<PrimaryPolicyAckOverride>();
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
public sealed class CachedPrimaryPolicyAckOverrideStore(MemoryPrimaryPolicyAckOverrideStore inner, PrimaryPolicyAckOverrideCache cache, int hits, int misses) : IPrimaryPolicyAckOverrideStore
{
    public MemoryPrimaryPolicyAckOverrideStore Inner { get; set; } = inner;
    public PrimaryPolicyAckOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryPolicyAckOverride? Find(string code)
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

    public void Save(PrimaryPolicyAckOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryPolicyAckOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary policy ack overrides. Empty fields match all.</summary>
public sealed record PrimaryPolicyAckOverrideQuery(string Title, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryPolicyAckOverride item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryPolicyAckOverride> Apply(List<PrimaryPolicyAckOverride> items)
    {
        var matched = Collections.Filter(items, (PrimaryPolicyAckOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary policy ack override store.</summary>
public static class PrimaryPolicyAckOverrideStore
{
    public static IPrimaryPolicyAckOverrideStore NewMemoryPrimaryPolicyAckOverrideStore()
    {
        return new MemoryPrimaryPolicyAckOverrideStore(
            new Dictionary<string, PrimaryPolicyAckOverride>(),
            new List<string>());
    }

    public static IPrimaryPolicyAckOverrideStore NewCachedPrimaryPolicyAckOverrideStore(MemoryPrimaryPolicyAckOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryPolicyAckOverrideStore(
            inner,
            PrimaryPolicyAckOverrideCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryPolicyAckOverride> FindPrimaryPolicyAckOverrides(IPrimaryPolicyAckOverrideStore store, PrimaryPolicyAckOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
