namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy ack manifests by code.</summary>
public interface IPolicyAckManifestStore
{
    PolicyAckManifest? Find(string code);
    void Save(PolicyAckManifest item);
    bool Remove(string code);
    List<PolicyAckManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAckManifestStore(Dictionary<string, PolicyAckManifest> items, List<string> order) : IPolicyAckManifestStore
{
    public Dictionary<string, PolicyAckManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAckManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAckManifest item)
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

    public List<PolicyAckManifest> List()
    {
        var outValue = new List<PolicyAckManifest>();
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
public sealed class CachedPolicyAckManifestStore(MemoryPolicyAckManifestStore inner, PolicyAckManifestCache cache, int hits, int misses) : IPolicyAckManifestStore
{
    public MemoryPolicyAckManifestStore Inner { get; set; } = inner;
    public PolicyAckManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAckManifest? Find(string code)
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

    public void Save(PolicyAckManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAckManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy ack manifests. Empty fields match all.</summary>
public sealed record PolicyAckManifestQuery(string Locale, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAckManifest item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyAckManifest> Apply(List<PolicyAckManifest> items)
    {
        var matched = Collections.Filter(items, (PolicyAckManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy ack manifest store.</summary>
public static class PolicyAckManifestStore
{
    public static IPolicyAckManifestStore NewMemoryPolicyAckManifestStore()
    {
        return new MemoryPolicyAckManifestStore(new Dictionary<string, PolicyAckManifest>(), new List<string>());
    }

    public static IPolicyAckManifestStore NewCachedPolicyAckManifestStore(MemoryPolicyAckManifestStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAckManifestStore(inner, PolicyAckManifestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PolicyAckManifest> FindPolicyAckManifests(IPolicyAckManifestStore store, PolicyAckManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
