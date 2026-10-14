namespace Acme.Workforce.Compensation.PayBand.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band tiers by code.</summary>
public interface IPayBandTierStore
{
    PayBandTier? Find(string code);
    void Save(PayBandTier item);
    bool Remove(string code);
    List<PayBandTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandTierStore(Dictionary<string, PayBandTier> items, List<string> order) : IPayBandTierStore
{
    public Dictionary<string, PayBandTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandTier item)
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

    public List<PayBandTier> List()
    {
        var outValue = new List<PayBandTier>();
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
public sealed class CachedPayBandTierStore(MemoryPayBandTierStore inner, PayBandTierCache cache, int hits, int misses) : IPayBandTierStore
{
    public MemoryPayBandTierStore Inner { get; set; } = inner;
    public PayBandTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandTier? Find(string code)
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

    public void Save(PayBandTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band tiers. Empty fields match all.</summary>
public sealed record PayBandTierQuery(string Title, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandTier item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandTier> Apply(List<PayBandTier> items)
    {
        var matched = Collections.Filter(items, (PayBandTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band tier store.</summary>
public static class PayBandTierStore
{
    public static IPayBandTierStore NewMemoryPayBandTierStore()
    {
        return new MemoryPayBandTierStore(new Dictionary<string, PayBandTier>(), new List<string>());
    }

    public static IPayBandTierStore NewCachedPayBandTierStore(MemoryPayBandTierStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandTierStore(inner, PayBandTierCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PayBandTier> FindPayBandTiers(IPayBandTierStore store, PayBandTierQuery query)
    {
        return query.Apply(store.List());
    }
}
