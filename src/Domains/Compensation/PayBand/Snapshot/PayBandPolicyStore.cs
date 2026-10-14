namespace Acme.Workforce.Compensation.PayBand.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band policies by code.</summary>
public interface IPayBandPolicyStore
{
    PayBandPolicy? Find(string code);
    void Save(PayBandPolicy item);
    bool Remove(string code);
    List<PayBandPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandPolicyStore(Dictionary<string, PayBandPolicy> items, List<string> order) : IPayBandPolicyStore
{
    public Dictionary<string, PayBandPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandPolicy item)
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

    public List<PayBandPolicy> List()
    {
        var outValue = new List<PayBandPolicy>();
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
public sealed class CachedPayBandPolicyStore(MemoryPayBandPolicyStore inner, PayBandPolicyCache cache, int hits, int misses) : IPayBandPolicyStore
{
    public MemoryPayBandPolicyStore Inner { get; set; } = inner;
    public PayBandPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandPolicy? Find(string code)
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

    public void Save(PayBandPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band policies. Empty fields match all.</summary>
public sealed record PayBandPolicyQuery(string Title, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandPolicy item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandPolicy> Apply(List<PayBandPolicy> items)
    {
        var matched = Collections.Filter(items, (PayBandPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band policy store.</summary>
public static class PayBandPolicyStore
{
    public static IPayBandPolicyStore NewMemoryPayBandPolicyStore()
    {
        return new MemoryPayBandPolicyStore(new Dictionary<string, PayBandPolicy>(), new List<string>());
    }

    public static IPayBandPolicyStore NewCachedPayBandPolicyStore(MemoryPayBandPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandPolicyStore(inner, PayBandPolicyCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PayBandPolicy> FindPayBandPolicies(IPayBandPolicyStore store, PayBandPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
