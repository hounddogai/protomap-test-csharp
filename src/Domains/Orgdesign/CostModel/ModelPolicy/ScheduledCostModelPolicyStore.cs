namespace Acme.Workforce.Orgdesign.CostModel.ModelPolicy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled cost model policies by code.</summary>
public interface IScheduledCostModelPolicyStore
{
    ScheduledCostModelPolicy? Find(string code);
    void Save(ScheduledCostModelPolicy item);
    bool Remove(string code);
    List<ScheduledCostModelPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledCostModelPolicyStore(Dictionary<string, ScheduledCostModelPolicy> items, List<string> order) : IScheduledCostModelPolicyStore
{
    public Dictionary<string, ScheduledCostModelPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledCostModelPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledCostModelPolicy item)
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

    public List<ScheduledCostModelPolicy> List()
    {
        var outValue = new List<ScheduledCostModelPolicy>();
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
public sealed class CachedScheduledCostModelPolicyStore(MemoryScheduledCostModelPolicyStore inner, ScheduledCostModelPolicyCache cache, int hits, int misses) : IScheduledCostModelPolicyStore
{
    public MemoryScheduledCostModelPolicyStore Inner { get; set; } = inner;
    public ScheduledCostModelPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledCostModelPolicy? Find(string code)
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

    public void Save(ScheduledCostModelPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledCostModelPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled cost model policies. Empty fields match all.</summary>
public sealed record ScheduledCostModelPolicyQuery(string Locale, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledCostModelPolicy item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledCostModelPolicy> Apply(List<ScheduledCostModelPolicy> items)
    {
        var matched = Collections.Filter(items, (ScheduledCostModelPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled cost model policy store.</summary>
public static class ScheduledCostModelPolicyStore
{
    public static IScheduledCostModelPolicyStore NewMemoryScheduledCostModelPolicyStore()
    {
        return new MemoryScheduledCostModelPolicyStore(
            new Dictionary<string, ScheduledCostModelPolicy>(),
            new List<string>());
    }

    public static IScheduledCostModelPolicyStore NewCachedScheduledCostModelPolicyStore(MemoryScheduledCostModelPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledCostModelPolicyStore(
            inner,
            ScheduledCostModelPolicyCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledCostModelPolicy> FindScheduledCostModelPolicies(IScheduledCostModelPolicyStore store, ScheduledCostModelPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
