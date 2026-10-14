namespace Acme.Workforce.Absence.Policy.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy adjustments by code.</summary>
public interface IPolicyAdjustmentStore
{
    PolicyAdjustment? Find(string code);
    void Save(PolicyAdjustment item);
    bool Remove(string code);
    List<PolicyAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAdjustmentStore(Dictionary<string, PolicyAdjustment> items, List<string> order) : IPolicyAdjustmentStore
{
    public Dictionary<string, PolicyAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAdjustment item)
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

    public List<PolicyAdjustment> List()
    {
        var outValue = new List<PolicyAdjustment>();
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
public sealed class CachedPolicyAdjustmentStore(MemoryPolicyAdjustmentStore inner, PolicyAdjustmentCache cache, int hits, int misses) : IPolicyAdjustmentStore
{
    public MemoryPolicyAdjustmentStore Inner { get; set; } = inner;
    public PolicyAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAdjustment? Find(string code)
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

    public void Save(PolicyAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy adjustments. Empty fields match all.</summary>
public sealed record PolicyAdjustmentQuery(string Unit, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAdjustment item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<PolicyAdjustment> Apply(List<PolicyAdjustment> items)
    {
        var matched = Collections.Filter(items, (PolicyAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy adjustment store.</summary>
public static class PolicyAdjustmentStore
{
    public static IPolicyAdjustmentStore NewMemoryPolicyAdjustmentStore()
    {
        return new MemoryPolicyAdjustmentStore(new Dictionary<string, PolicyAdjustment>(), new List<string>());
    }

    public static IPolicyAdjustmentStore NewCachedPolicyAdjustmentStore(MemoryPolicyAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAdjustmentStore(inner, PolicyAdjustmentCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PolicyAdjustment> FindPolicyAdjustments(IPolicyAdjustmentStore store, PolicyAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
