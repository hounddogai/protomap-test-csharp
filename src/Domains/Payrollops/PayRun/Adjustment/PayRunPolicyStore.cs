namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run policies by code.</summary>
public interface IPayRunPolicyStore
{
    PayRunPolicy? Find(string code);
    void Save(PayRunPolicy item);
    bool Remove(string code);
    List<PayRunPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunPolicyStore(Dictionary<string, PayRunPolicy> items, List<string> order) : IPayRunPolicyStore
{
    public Dictionary<string, PayRunPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunPolicy item)
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

    public List<PayRunPolicy> List()
    {
        var outValue = new List<PayRunPolicy>();
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
public sealed class CachedPayRunPolicyStore(MemoryPayRunPolicyStore inner, PayRunPolicyCache cache, int hits, int misses) : IPayRunPolicyStore
{
    public MemoryPayRunPolicyStore Inner { get; set; } = inner;
    public PayRunPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunPolicy? Find(string code)
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

    public void Save(PayRunPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run policies. Empty fields match all.</summary>
public sealed record PayRunPolicyQuery(string Locale, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunPolicy item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayRunPolicy> Apply(List<PayRunPolicy> items)
    {
        var matched = Collections.Filter(items, (PayRunPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run policy store.</summary>
public static class PayRunPolicyStore
{
    public static IPayRunPolicyStore NewMemoryPayRunPolicyStore()
    {
        return new MemoryPayRunPolicyStore(new Dictionary<string, PayRunPolicy>(), new List<string>());
    }

    public static IPayRunPolicyStore NewCachedPayRunPolicyStore(MemoryPayRunPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunPolicyStore(inner, PayRunPolicyCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PayRunPolicy> FindPayRunPolicies(IPayRunPolicyStore store, PayRunPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
