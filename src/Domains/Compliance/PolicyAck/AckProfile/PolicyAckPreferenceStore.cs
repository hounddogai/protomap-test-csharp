namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy ack preferences by code.</summary>
public interface IPolicyAckPreferenceStore
{
    PolicyAckPreference? Find(string code);
    void Save(PolicyAckPreference item);
    bool Remove(string code);
    List<PolicyAckPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAckPreferenceStore(Dictionary<string, PolicyAckPreference> items, List<string> order) : IPolicyAckPreferenceStore
{
    public Dictionary<string, PolicyAckPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAckPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAckPreference item)
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

    public List<PolicyAckPreference> List()
    {
        var outValue = new List<PolicyAckPreference>();
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
public sealed class CachedPolicyAckPreferenceStore(MemoryPolicyAckPreferenceStore inner, PolicyAckPreferenceCache cache, int hits, int misses) : IPolicyAckPreferenceStore
{
    public MemoryPolicyAckPreferenceStore Inner { get; set; } = inner;
    public PolicyAckPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAckPreference? Find(string code)
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

    public void Save(PolicyAckPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAckPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy ack preferences. Empty fields match all.</summary>
public sealed record PolicyAckPreferenceQuery(string Name, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAckPreference item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyAckPreference> Apply(List<PolicyAckPreference> items)
    {
        var matched = Collections.Filter(items, (PolicyAckPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy ack preference store.</summary>
public static class PolicyAckPreferenceStore
{
    public static IPolicyAckPreferenceStore NewMemoryPolicyAckPreferenceStore()
    {
        return new MemoryPolicyAckPreferenceStore(new Dictionary<string, PolicyAckPreference>(), new List<string>());
    }

    public static IPolicyAckPreferenceStore NewCachedPolicyAckPreferenceStore(MemoryPolicyAckPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAckPreferenceStore(inner, PolicyAckPreferenceCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PolicyAckPreference> FindPolicyAckPreferences(IPolicyAckPreferenceStore store, PolicyAckPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
