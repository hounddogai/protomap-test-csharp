namespace Acme.Workforce.Contractors.Engagement.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps engagement policies by code.</summary>
public interface IEngagementPolicyStore
{
    EngagementPolicy? Find(string code);
    void Save(EngagementPolicy item);
    bool Remove(string code);
    List<EngagementPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEngagementPolicyStore(Dictionary<string, EngagementPolicy> items, List<string> order) : IEngagementPolicyStore
{
    public Dictionary<string, EngagementPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EngagementPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EngagementPolicy item)
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

    public List<EngagementPolicy> List()
    {
        var outValue = new List<EngagementPolicy>();
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
public sealed class CachedEngagementPolicyStore(MemoryEngagementPolicyStore inner, EngagementPolicyCache cache, int hits, int misses) : IEngagementPolicyStore
{
    public MemoryEngagementPolicyStore Inner { get; set; } = inner;
    public EngagementPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EngagementPolicy? Find(string code)
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

    public void Save(EngagementPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EngagementPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects engagement policies. Empty fields match all.</summary>
public sealed record EngagementPolicyQuery(string Team, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(EngagementPolicy item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<EngagementPolicy> Apply(List<EngagementPolicy> items)
    {
        var matched = Collections.Filter(items, (EngagementPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of engagement policy store.</summary>
public static class EngagementPolicyStore
{
    public static IEngagementPolicyStore NewMemoryEngagementPolicyStore()
    {
        return new MemoryEngagementPolicyStore(new Dictionary<string, EngagementPolicy>(), new List<string>());
    }

    public static IEngagementPolicyStore NewCachedEngagementPolicyStore(MemoryEngagementPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedEngagementPolicyStore(inner, EngagementPolicyCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<EngagementPolicy> FindEngagementPolicies(IEngagementPolicyStore store, EngagementPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
