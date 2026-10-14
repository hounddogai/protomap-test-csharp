namespace Acme.Workforce.Compensation.BonusPool.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool policies by code.</summary>
public interface IBonusPoolPolicyStore
{
    BonusPoolPolicy? Find(string code);
    void Save(BonusPoolPolicy item);
    bool Remove(string code);
    List<BonusPoolPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolPolicyStore(Dictionary<string, BonusPoolPolicy> items, List<string> order) : IBonusPoolPolicyStore
{
    public Dictionary<string, BonusPoolPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolPolicy item)
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

    public List<BonusPoolPolicy> List()
    {
        var outValue = new List<BonusPoolPolicy>();
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
public sealed class CachedBonusPoolPolicyStore(MemoryBonusPoolPolicyStore inner, BonusPoolPolicyCache cache, int hits, int misses) : IBonusPoolPolicyStore
{
    public MemoryBonusPoolPolicyStore Inner { get; set; } = inner;
    public BonusPoolPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolPolicy? Find(string code)
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

    public void Save(BonusPoolPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool policies. Empty fields match all.</summary>
public sealed record BonusPoolPolicyQuery(string Name, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolPolicy item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolPolicy> Apply(List<BonusPoolPolicy> items)
    {
        var matched = Collections.Filter(items, (BonusPoolPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool policy store.</summary>
public static class BonusPoolPolicyStore
{
    public static IBonusPoolPolicyStore NewMemoryBonusPoolPolicyStore()
    {
        return new MemoryBonusPoolPolicyStore(new Dictionary<string, BonusPoolPolicy>(), new List<string>());
    }

    public static IBonusPoolPolicyStore NewCachedBonusPoolPolicyStore(MemoryBonusPoolPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolPolicyStore(inner, BonusPoolPolicyCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BonusPoolPolicy> FindBonusPoolPolicies(IBonusPoolPolicyStore store, BonusPoolPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
