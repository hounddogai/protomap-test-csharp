namespace Acme.Workforce.Absence.Policy.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy mappings by code.</summary>
public interface IPolicyMappingStore
{
    PolicyMapping? Find(string code);
    void Save(PolicyMapping item);
    bool Remove(string code);
    List<PolicyMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyMappingStore(Dictionary<string, PolicyMapping> items, List<string> order) : IPolicyMappingStore
{
    public Dictionary<string, PolicyMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyMapping item)
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

    public List<PolicyMapping> List()
    {
        var outValue = new List<PolicyMapping>();
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
public sealed class CachedPolicyMappingStore(MemoryPolicyMappingStore inner, PolicyMappingCache cache, int hits, int misses) : IPolicyMappingStore
{
    public MemoryPolicyMappingStore Inner { get; set; } = inner;
    public PolicyMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyMapping? Find(string code)
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

    public void Save(PolicyMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy mappings. Empty fields match all.</summary>
public sealed record PolicyMappingQuery(string Unit, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyMapping item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<PolicyMapping> Apply(List<PolicyMapping> items)
    {
        var matched = Collections.Filter(items, (PolicyMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy mapping store.</summary>
public static class PolicyMappingStore
{
    public static IPolicyMappingStore NewMemoryPolicyMappingStore()
    {
        return new MemoryPolicyMappingStore(new Dictionary<string, PolicyMapping>(), new List<string>());
    }

    public static IPolicyMappingStore NewCachedPolicyMappingStore(MemoryPolicyMappingStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyMappingStore(inner, PolicyMappingCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PolicyMapping> FindPolicyMappings(IPolicyMappingStore store, PolicyMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
