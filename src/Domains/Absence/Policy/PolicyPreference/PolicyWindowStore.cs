namespace Acme.Workforce.Absence.Policy.PolicyPreference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy windows by code.</summary>
public interface IPolicyWindowStore
{
    PolicyWindow? Find(string code);
    void Save(PolicyWindow item);
    bool Remove(string code);
    List<PolicyWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyWindowStore(Dictionary<string, PolicyWindow> items, List<string> order) : IPolicyWindowStore
{
    public Dictionary<string, PolicyWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyWindow item)
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

    public List<PolicyWindow> List()
    {
        var outValue = new List<PolicyWindow>();
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
public sealed class CachedPolicyWindowStore(MemoryPolicyWindowStore inner, PolicyWindowCache cache, int hits, int misses) : IPolicyWindowStore
{
    public MemoryPolicyWindowStore Inner { get; set; } = inner;
    public PolicyWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyWindow? Find(string code)
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

    public void Save(PolicyWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy windows. Empty fields match all.</summary>
public sealed record PolicyWindowQuery(string Name, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyWindow item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyWindow> Apply(List<PolicyWindow> items)
    {
        var matched = Collections.Filter(items, (PolicyWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy window store.</summary>
public static class PolicyWindowStore
{
    public static IPolicyWindowStore NewMemoryPolicyWindowStore()
    {
        return new MemoryPolicyWindowStore(new Dictionary<string, PolicyWindow>(), new List<string>());
    }

    public static IPolicyWindowStore NewCachedPolicyWindowStore(MemoryPolicyWindowStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyWindowStore(inner, PolicyWindowCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PolicyWindow> FindPolicyWindows(IPolicyWindowStore store, PolicyWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
