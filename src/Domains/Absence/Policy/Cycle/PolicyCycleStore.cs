namespace Acme.Workforce.Absence.Policy.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy cycles by code.</summary>
public interface IPolicyCycleStore
{
    PolicyCycle? Find(string code);
    void Save(PolicyCycle item);
    bool Remove(string code);
    List<PolicyCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyCycleStore(Dictionary<string, PolicyCycle> items, List<string> order) : IPolicyCycleStore
{
    public Dictionary<string, PolicyCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyCycle item)
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

    public List<PolicyCycle> List()
    {
        var outValue = new List<PolicyCycle>();
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
public sealed class CachedPolicyCycleStore(MemoryPolicyCycleStore inner, PolicyCycleCache cache, int hits, int misses) : IPolicyCycleStore
{
    public MemoryPolicyCycleStore Inner { get; set; } = inner;
    public PolicyCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyCycle? Find(string code)
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

    public void Save(PolicyCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy cycles. Empty fields match all.</summary>
public sealed record PolicyCycleQuery(string Name, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyCycle item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyCycle> Apply(List<PolicyCycle> items)
    {
        var matched = Collections.Filter(items, (PolicyCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy cycle store.</summary>
public static class PolicyCycleStore
{
    public static IPolicyCycleStore NewMemoryPolicyCycleStore()
    {
        return new MemoryPolicyCycleStore(new Dictionary<string, PolicyCycle>(), new List<string>());
    }

    public static IPolicyCycleStore NewCachedPolicyCycleStore(MemoryPolicyCycleStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyCycleStore(inner, PolicyCycleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PolicyCycle> FindPolicyCycles(IPolicyCycleStore store, PolicyCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
