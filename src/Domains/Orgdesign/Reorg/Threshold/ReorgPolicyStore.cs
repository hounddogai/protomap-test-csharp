namespace Acme.Workforce.Orgdesign.Reorg.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg policies by code.</summary>
public interface IReorgPolicyStore
{
    ReorgPolicy? Find(string code);
    void Save(ReorgPolicy item);
    bool Remove(string code);
    List<ReorgPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgPolicyStore(Dictionary<string, ReorgPolicy> items, List<string> order) : IReorgPolicyStore
{
    public Dictionary<string, ReorgPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgPolicy item)
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

    public List<ReorgPolicy> List()
    {
        var outValue = new List<ReorgPolicy>();
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
public sealed class CachedReorgPolicyStore(MemoryReorgPolicyStore inner, ReorgPolicyCache cache, int hits, int misses) : IReorgPolicyStore
{
    public MemoryReorgPolicyStore Inner { get; set; } = inner;
    public ReorgPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgPolicy? Find(string code)
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

    public void Save(ReorgPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg policies. Empty fields match all.</summary>
public sealed record ReorgPolicyQuery(string Team, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgPolicy item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<ReorgPolicy> Apply(List<ReorgPolicy> items)
    {
        var matched = Collections.Filter(items, (ReorgPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg policy store.</summary>
public static class ReorgPolicyStore
{
    public static IReorgPolicyStore NewMemoryReorgPolicyStore()
    {
        return new MemoryReorgPolicyStore(new Dictionary<string, ReorgPolicy>(), new List<string>());
    }

    public static IReorgPolicyStore NewCachedReorgPolicyStore(MemoryReorgPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedReorgPolicyStore(inner, ReorgPolicyCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ReorgPolicy> FindReorgPolicies(IReorgPolicyStore store, ReorgPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
