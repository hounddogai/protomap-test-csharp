namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount policies by code.</summary>
public interface IHeadcountPolicyStore
{
    HeadcountPolicy? Find(string code);
    void Save(HeadcountPolicy item);
    bool Remove(string code);
    List<HeadcountPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountPolicyStore(Dictionary<string, HeadcountPolicy> items, List<string> order) : IHeadcountPolicyStore
{
    public Dictionary<string, HeadcountPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountPolicy item)
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

    public List<HeadcountPolicy> List()
    {
        var outValue = new List<HeadcountPolicy>();
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
public sealed class CachedHeadcountPolicyStore(MemoryHeadcountPolicyStore inner, HeadcountPolicyCache cache, int hits, int misses) : IHeadcountPolicyStore
{
    public MemoryHeadcountPolicyStore Inner { get; set; } = inner;
    public HeadcountPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountPolicy? Find(string code)
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

    public void Save(HeadcountPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount policies. Empty fields match all.</summary>
public sealed record HeadcountPolicyQuery(string Channel, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountPolicy item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountPolicy> Apply(List<HeadcountPolicy> items)
    {
        var matched = Collections.Filter(items, (HeadcountPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount policy store.</summary>
public static class HeadcountPolicyStore
{
    public static IHeadcountPolicyStore NewMemoryHeadcountPolicyStore()
    {
        return new MemoryHeadcountPolicyStore(new Dictionary<string, HeadcountPolicy>(), new List<string>());
    }

    public static IHeadcountPolicyStore NewCachedHeadcountPolicyStore(MemoryHeadcountPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountPolicyStore(inner, HeadcountPolicyCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<HeadcountPolicy> FindHeadcountPolicies(IHeadcountPolicyStore store, HeadcountPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
