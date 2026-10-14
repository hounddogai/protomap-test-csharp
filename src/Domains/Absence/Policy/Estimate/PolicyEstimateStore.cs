namespace Acme.Workforce.Absence.Policy.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy estimates by code.</summary>
public interface IPolicyEstimateStore
{
    PolicyEstimate? Find(string code);
    void Save(PolicyEstimate item);
    bool Remove(string code);
    List<PolicyEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyEstimateStore(Dictionary<string, PolicyEstimate> items, List<string> order) : IPolicyEstimateStore
{
    public Dictionary<string, PolicyEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyEstimate item)
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

    public List<PolicyEstimate> List()
    {
        var outValue = new List<PolicyEstimate>();
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
public sealed class CachedPolicyEstimateStore(MemoryPolicyEstimateStore inner, PolicyEstimateCache cache, int hits, int misses) : IPolicyEstimateStore
{
    public MemoryPolicyEstimateStore Inner { get; set; } = inner;
    public PolicyEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyEstimate? Find(string code)
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

    public void Save(PolicyEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy estimates. Empty fields match all.</summary>
public sealed record PolicyEstimateQuery(string Title, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyEstimate item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyEstimate> Apply(List<PolicyEstimate> items)
    {
        var matched = Collections.Filter(items, (PolicyEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy estimate store.</summary>
public static class PolicyEstimateStore
{
    public static IPolicyEstimateStore NewMemoryPolicyEstimateStore()
    {
        return new MemoryPolicyEstimateStore(new Dictionary<string, PolicyEstimate>(), new List<string>());
    }

    public static IPolicyEstimateStore NewCachedPolicyEstimateStore(MemoryPolicyEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyEstimateStore(inner, PolicyEstimateCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PolicyEstimate> FindPolicyEstimates(IPolicyEstimateStore store, PolicyEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
