namespace Acme.Workforce.Payrollops.Deduction.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped deduction policies by code.</summary>
public interface ICappedDeductionPolicyStore
{
    CappedDeductionPolicy? Find(string code);
    void Save(CappedDeductionPolicy item);
    bool Remove(string code);
    List<CappedDeductionPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedDeductionPolicyStore(Dictionary<string, CappedDeductionPolicy> items, List<string> order) : ICappedDeductionPolicyStore
{
    public Dictionary<string, CappedDeductionPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedDeductionPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedDeductionPolicy item)
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

    public List<CappedDeductionPolicy> List()
    {
        var outValue = new List<CappedDeductionPolicy>();
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
public sealed class CachedCappedDeductionPolicyStore(MemoryCappedDeductionPolicyStore inner, CappedDeductionPolicyCache cache, int hits, int misses) : ICappedDeductionPolicyStore
{
    public MemoryCappedDeductionPolicyStore Inner { get; set; } = inner;
    public CappedDeductionPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedDeductionPolicy? Find(string code)
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

    public void Save(CappedDeductionPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedDeductionPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped deduction policies. Empty fields match all.</summary>
public sealed record CappedDeductionPolicyQuery(string Category, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(CappedDeductionPolicy item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedDeductionPolicy> Apply(List<CappedDeductionPolicy> items)
    {
        var matched = Collections.Filter(items, (CappedDeductionPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped deduction policy store.</summary>
public static class CappedDeductionPolicyStore
{
    public static ICappedDeductionPolicyStore NewMemoryCappedDeductionPolicyStore()
    {
        return new MemoryCappedDeductionPolicyStore(
            new Dictionary<string, CappedDeductionPolicy>(),
            new List<string>());
    }

    public static ICappedDeductionPolicyStore NewCachedCappedDeductionPolicyStore(MemoryCappedDeductionPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedCappedDeductionPolicyStore(inner, CappedDeductionPolicyCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<CappedDeductionPolicy> FindCappedDeductionPolicies(ICappedDeductionPolicyStore store, CappedDeductionPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
