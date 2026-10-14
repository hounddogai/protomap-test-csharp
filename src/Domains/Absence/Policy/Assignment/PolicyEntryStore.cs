namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy entries by code.</summary>
public interface IPolicyEntryStore
{
    PolicyEntry? Find(string code);
    void Save(PolicyEntry item);
    bool Remove(string code);
    List<PolicyEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyEntryStore(Dictionary<string, PolicyEntry> items, List<string> order) : IPolicyEntryStore
{
    public Dictionary<string, PolicyEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyEntry item)
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

    public List<PolicyEntry> List()
    {
        var outValue = new List<PolicyEntry>();
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
public sealed class CachedPolicyEntryStore(MemoryPolicyEntryStore inner, PolicyEntryCache cache, int hits, int misses) : IPolicyEntryStore
{
    public MemoryPolicyEntryStore Inner { get; set; } = inner;
    public PolicyEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyEntry? Find(string code)
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

    public void Save(PolicyEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy entries. Empty fields match all.</summary>
public sealed record PolicyEntryQuery(string Name, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyEntry item)
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

    public List<PolicyEntry> Apply(List<PolicyEntry> items)
    {
        var matched = Collections.Filter(items, (PolicyEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy entry store.</summary>
public static class PolicyEntryStore
{
    public static IPolicyEntryStore NewMemoryPolicyEntryStore()
    {
        return new MemoryPolicyEntryStore(new Dictionary<string, PolicyEntry>(), new List<string>());
    }

    public static IPolicyEntryStore NewCachedPolicyEntryStore(MemoryPolicyEntryStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyEntryStore(inner, PolicyEntryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PolicyEntry> FindPolicyEntries(IPolicyEntryStore store, PolicyEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
