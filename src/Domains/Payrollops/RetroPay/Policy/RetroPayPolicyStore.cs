namespace Acme.Workforce.Payrollops.RetroPay.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay policies by code.</summary>
public interface IRetroPayPolicyStore
{
    RetroPayPolicy? Find(string code);
    void Save(RetroPayPolicy item);
    bool Remove(string code);
    List<RetroPayPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayPolicyStore(Dictionary<string, RetroPayPolicy> items, List<string> order) : IRetroPayPolicyStore
{
    public Dictionary<string, RetroPayPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayPolicy item)
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

    public List<RetroPayPolicy> List()
    {
        var outValue = new List<RetroPayPolicy>();
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
public sealed class CachedRetroPayPolicyStore(MemoryRetroPayPolicyStore inner, RetroPayPolicyCache cache, int hits, int misses) : IRetroPayPolicyStore
{
    public MemoryRetroPayPolicyStore Inner { get; set; } = inner;
    public RetroPayPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayPolicy? Find(string code)
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

    public void Save(RetroPayPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay policies. Empty fields match all.</summary>
public sealed record RetroPayPolicyQuery(string Department, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayPolicy item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RetroPayPolicy> Apply(List<RetroPayPolicy> items)
    {
        var matched = Collections.Filter(items, (RetroPayPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay policy store.</summary>
public static class RetroPayPolicyStore
{
    public static IRetroPayPolicyStore NewMemoryRetroPayPolicyStore()
    {
        return new MemoryRetroPayPolicyStore(new Dictionary<string, RetroPayPolicy>(), new List<string>());
    }

    public static IRetroPayPolicyStore NewCachedRetroPayPolicyStore(MemoryRetroPayPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayPolicyStore(inner, RetroPayPolicyCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RetroPayPolicy> FindRetroPayPolicies(IRetroPayPolicyStore store, RetroPayPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
