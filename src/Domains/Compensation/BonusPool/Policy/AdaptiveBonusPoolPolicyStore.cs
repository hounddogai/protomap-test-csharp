namespace Acme.Workforce.Compensation.BonusPool.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive bonus pool policies by code.</summary>
public interface IAdaptiveBonusPoolPolicyStore
{
    AdaptiveBonusPoolPolicy? Find(string code);
    void Save(AdaptiveBonusPoolPolicy item);
    bool Remove(string code);
    List<AdaptiveBonusPoolPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveBonusPoolPolicyStore(Dictionary<string, AdaptiveBonusPoolPolicy> items, List<string> order) : IAdaptiveBonusPoolPolicyStore
{
    public Dictionary<string, AdaptiveBonusPoolPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveBonusPoolPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveBonusPoolPolicy item)
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

    public List<AdaptiveBonusPoolPolicy> List()
    {
        var outValue = new List<AdaptiveBonusPoolPolicy>();
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
public sealed class CachedAdaptiveBonusPoolPolicyStore(MemoryAdaptiveBonusPoolPolicyStore inner, AdaptiveBonusPoolPolicyCache cache, int hits, int misses) : IAdaptiveBonusPoolPolicyStore
{
    public MemoryAdaptiveBonusPoolPolicyStore Inner { get; set; } = inner;
    public AdaptiveBonusPoolPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveBonusPoolPolicy? Find(string code)
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

    public void Save(AdaptiveBonusPoolPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveBonusPoolPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive bonus pool policies. Empty fields match all.</summary>
public sealed record AdaptiveBonusPoolPolicyQuery(string Title, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveBonusPoolPolicy item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveBonusPoolPolicy> Apply(List<AdaptiveBonusPoolPolicy> items)
    {
        var matched = Collections.Filter(items, (AdaptiveBonusPoolPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive bonus pool policy store.</summary>
public static class AdaptiveBonusPoolPolicyStore
{
    public static IAdaptiveBonusPoolPolicyStore NewMemoryAdaptiveBonusPoolPolicyStore()
    {
        return new MemoryAdaptiveBonusPoolPolicyStore(
            new Dictionary<string, AdaptiveBonusPoolPolicy>(),
            new List<string>());
    }

    public static IAdaptiveBonusPoolPolicyStore NewCachedAdaptiveBonusPoolPolicyStore(MemoryAdaptiveBonusPoolPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveBonusPoolPolicyStore(
            inner,
            AdaptiveBonusPoolPolicyCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveBonusPoolPolicy> FindAdaptiveBonusPoolPolicies(IAdaptiveBonusPoolPolicyStore store, AdaptiveBonusPoolPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
