namespace Acme.Workforce.Absence.Policy.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy bundles by code.</summary>
public interface IPolicyBundleStore
{
    PolicyBundle? Find(string code);
    void Save(PolicyBundle item);
    bool Remove(string code);
    List<PolicyBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyBundleStore(Dictionary<string, PolicyBundle> items, List<string> order) : IPolicyBundleStore
{
    public Dictionary<string, PolicyBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyBundle item)
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

    public List<PolicyBundle> List()
    {
        var outValue = new List<PolicyBundle>();
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
public sealed class CachedPolicyBundleStore(MemoryPolicyBundleStore inner, PolicyBundleCache cache, int hits, int misses) : IPolicyBundleStore
{
    public MemoryPolicyBundleStore Inner { get; set; } = inner;
    public PolicyBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyBundle? Find(string code)
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

    public void Save(PolicyBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy bundles. Empty fields match all.</summary>
public sealed record PolicyBundleQuery(string Unit, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyBundle item)
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

    public List<PolicyBundle> Apply(List<PolicyBundle> items)
    {
        var matched = Collections.Filter(items, (PolicyBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy bundle store.</summary>
public static class PolicyBundleStore
{
    public static IPolicyBundleStore NewMemoryPolicyBundleStore()
    {
        return new MemoryPolicyBundleStore(new Dictionary<string, PolicyBundle>(), new List<string>());
    }

    public static IPolicyBundleStore NewCachedPolicyBundleStore(MemoryPolicyBundleStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyBundleStore(inner, PolicyBundleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PolicyBundle> FindPolicyBundles(IPolicyBundleStore store, PolicyBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
