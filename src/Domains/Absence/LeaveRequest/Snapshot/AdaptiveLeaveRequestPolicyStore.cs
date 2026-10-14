namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive leave request policies by code.</summary>
public interface IAdaptiveLeaveRequestPolicyStore
{
    AdaptiveLeaveRequestPolicy? Find(string code);
    void Save(AdaptiveLeaveRequestPolicy item);
    bool Remove(string code);
    List<AdaptiveLeaveRequestPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveLeaveRequestPolicyStore(Dictionary<string, AdaptiveLeaveRequestPolicy> items, List<string> order) : IAdaptiveLeaveRequestPolicyStore
{
    public Dictionary<string, AdaptiveLeaveRequestPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveLeaveRequestPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveLeaveRequestPolicy item)
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

    public List<AdaptiveLeaveRequestPolicy> List()
    {
        var outValue = new List<AdaptiveLeaveRequestPolicy>();
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
public sealed class CachedAdaptiveLeaveRequestPolicyStore(MemoryAdaptiveLeaveRequestPolicyStore inner, AdaptiveLeaveRequestPolicyCache cache, int hits, int misses) : IAdaptiveLeaveRequestPolicyStore
{
    public MemoryAdaptiveLeaveRequestPolicyStore Inner { get; set; } = inner;
    public AdaptiveLeaveRequestPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveLeaveRequestPolicy? Find(string code)
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

    public void Save(AdaptiveLeaveRequestPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveLeaveRequestPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive leave request policies. Empty fields match all.</summary>
public sealed record AdaptiveLeaveRequestPolicyQuery(string Category, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveLeaveRequestPolicy item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveLeaveRequestPolicy> Apply(List<AdaptiveLeaveRequestPolicy> items)
    {
        var matched = Collections.Filter(items, (AdaptiveLeaveRequestPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive leave request policy store.</summary>
public static class AdaptiveLeaveRequestPolicyStore
{
    public static IAdaptiveLeaveRequestPolicyStore NewMemoryAdaptiveLeaveRequestPolicyStore()
    {
        return new MemoryAdaptiveLeaveRequestPolicyStore(
            new Dictionary<string, AdaptiveLeaveRequestPolicy>(),
            new List<string>());
    }

    public static IAdaptiveLeaveRequestPolicyStore NewCachedAdaptiveLeaveRequestPolicyStore(MemoryAdaptiveLeaveRequestPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveLeaveRequestPolicyStore(
            inner,
            AdaptiveLeaveRequestPolicyCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveLeaveRequestPolicy> FindAdaptiveLeaveRequestPolicies(IAdaptiveLeaveRequestPolicyStore store, AdaptiveLeaveRequestPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
