namespace Acme.Workforce.Absence.Accrual.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual accrual policies by code.</summary>
public interface IManualAccrualPolicyStore
{
    ManualAccrualPolicy? Find(string code);
    void Save(ManualAccrualPolicy item);
    bool Remove(string code);
    List<ManualAccrualPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualAccrualPolicyStore(Dictionary<string, ManualAccrualPolicy> items, List<string> order) : IManualAccrualPolicyStore
{
    public Dictionary<string, ManualAccrualPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualAccrualPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualAccrualPolicy item)
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

    public List<ManualAccrualPolicy> List()
    {
        var outValue = new List<ManualAccrualPolicy>();
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
public sealed class CachedManualAccrualPolicyStore(MemoryManualAccrualPolicyStore inner, ManualAccrualPolicyCache cache, int hits, int misses) : IManualAccrualPolicyStore
{
    public MemoryManualAccrualPolicyStore Inner { get; set; } = inner;
    public ManualAccrualPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualAccrualPolicy? Find(string code)
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

    public void Save(ManualAccrualPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualAccrualPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual accrual policies. Empty fields match all.</summary>
public sealed record ManualAccrualPolicyQuery(string Source, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(ManualAccrualPolicy item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<ManualAccrualPolicy> Apply(List<ManualAccrualPolicy> items)
    {
        var matched = Collections.Filter(items, (ManualAccrualPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual accrual policy store.</summary>
public static class ManualAccrualPolicyStore
{
    public static IManualAccrualPolicyStore NewMemoryManualAccrualPolicyStore()
    {
        return new MemoryManualAccrualPolicyStore(new Dictionary<string, ManualAccrualPolicy>(), new List<string>());
    }

    public static IManualAccrualPolicyStore NewCachedManualAccrualPolicyStore(MemoryManualAccrualPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedManualAccrualPolicyStore(inner, ManualAccrualPolicyCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ManualAccrualPolicy> FindManualAccrualPolicies(IManualAccrualPolicyStore store, ManualAccrualPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
