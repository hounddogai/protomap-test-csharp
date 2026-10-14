namespace Acme.Workforce.Compliance.PolicyAck.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy ack templates by code.</summary>
public interface IPolicyAckTemplateStore
{
    PolicyAckTemplate? Find(string code);
    void Save(PolicyAckTemplate item);
    bool Remove(string code);
    List<PolicyAckTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAckTemplateStore(Dictionary<string, PolicyAckTemplate> items, List<string> order) : IPolicyAckTemplateStore
{
    public Dictionary<string, PolicyAckTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAckTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAckTemplate item)
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

    public List<PolicyAckTemplate> List()
    {
        var outValue = new List<PolicyAckTemplate>();
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
public sealed class CachedPolicyAckTemplateStore(MemoryPolicyAckTemplateStore inner, PolicyAckTemplateCache cache, int hits, int misses) : IPolicyAckTemplateStore
{
    public MemoryPolicyAckTemplateStore Inner { get; set; } = inner;
    public PolicyAckTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAckTemplate? Find(string code)
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

    public void Save(PolicyAckTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAckTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy ack templates. Empty fields match all.</summary>
public sealed record PolicyAckTemplateQuery(string Name, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAckTemplate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyAckTemplate> Apply(List<PolicyAckTemplate> items)
    {
        var matched = Collections.Filter(items, (PolicyAckTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy ack template store.</summary>
public static class PolicyAckTemplateStore
{
    public static IPolicyAckTemplateStore NewMemoryPolicyAckTemplateStore()
    {
        return new MemoryPolicyAckTemplateStore(new Dictionary<string, PolicyAckTemplate>(), new List<string>());
    }

    public static IPolicyAckTemplateStore NewCachedPolicyAckTemplateStore(MemoryPolicyAckTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAckTemplateStore(inner, PolicyAckTemplateCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PolicyAckTemplate> FindPolicyAckTemplates(IPolicyAckTemplateStore store, PolicyAckTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
