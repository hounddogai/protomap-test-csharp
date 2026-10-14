namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending bonus pool policies by code.</summary>
public interface IPendingBonusPoolPolicyStore
{
    PendingBonusPoolPolicy? Find(string code);
    void Save(PendingBonusPoolPolicy item);
    bool Remove(string code);
    List<PendingBonusPoolPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingBonusPoolPolicyStore(Dictionary<string, PendingBonusPoolPolicy> items, List<string> order) : IPendingBonusPoolPolicyStore
{
    public Dictionary<string, PendingBonusPoolPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingBonusPoolPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingBonusPoolPolicy item)
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

    public List<PendingBonusPoolPolicy> List()
    {
        var outValue = new List<PendingBonusPoolPolicy>();
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
public sealed class CachedPendingBonusPoolPolicyStore(MemoryPendingBonusPoolPolicyStore inner, PendingBonusPoolPolicyCache cache, int hits, int misses) : IPendingBonusPoolPolicyStore
{
    public MemoryPendingBonusPoolPolicyStore Inner { get; set; } = inner;
    public PendingBonusPoolPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingBonusPoolPolicy? Find(string code)
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

    public void Save(PendingBonusPoolPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingBonusPoolPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending bonus pool policies. Empty fields match all.</summary>
public sealed record PendingBonusPoolPolicyQuery(string Unit, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(PendingBonusPoolPolicy item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<PendingBonusPoolPolicy> Apply(List<PendingBonusPoolPolicy> items)
    {
        var matched = Collections.Filter(items, (PendingBonusPoolPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending bonus pool policy store.</summary>
public static class PendingBonusPoolPolicyStore
{
    public static IPendingBonusPoolPolicyStore NewMemoryPendingBonusPoolPolicyStore()
    {
        return new MemoryPendingBonusPoolPolicyStore(
            new Dictionary<string, PendingBonusPoolPolicy>(),
            new List<string>());
    }

    public static IPendingBonusPoolPolicyStore NewCachedPendingBonusPoolPolicyStore(MemoryPendingBonusPoolPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedPendingBonusPoolPolicyStore(
            inner,
            PendingBonusPoolPolicyCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<PendingBonusPoolPolicy> FindPendingBonusPoolPolicies(IPendingBonusPoolPolicyStore store, PendingBonusPoolPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
