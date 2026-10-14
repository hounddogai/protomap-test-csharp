namespace Acme.Workforce.Compensation.PayBand.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band thresholds by code.</summary>
public interface IPayBandThresholdStore
{
    PayBandThreshold? Find(string code);
    void Save(PayBandThreshold item);
    bool Remove(string code);
    List<PayBandThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandThresholdStore(Dictionary<string, PayBandThreshold> items, List<string> order) : IPayBandThresholdStore
{
    public Dictionary<string, PayBandThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandThreshold item)
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

    public List<PayBandThreshold> List()
    {
        var outValue = new List<PayBandThreshold>();
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
public sealed class CachedPayBandThresholdStore(MemoryPayBandThresholdStore inner, PayBandThresholdCache cache, int hits, int misses) : IPayBandThresholdStore
{
    public MemoryPayBandThresholdStore Inner { get; set; } = inner;
    public PayBandThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandThreshold? Find(string code)
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

    public void Save(PayBandThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band thresholds. Empty fields match all.</summary>
public sealed record PayBandThresholdQuery(string Team, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandThreshold item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandThreshold> Apply(List<PayBandThreshold> items)
    {
        var matched = Collections.Filter(items, (PayBandThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band threshold store.</summary>
public static class PayBandThresholdStore
{
    public static IPayBandThresholdStore NewMemoryPayBandThresholdStore()
    {
        return new MemoryPayBandThresholdStore(new Dictionary<string, PayBandThreshold>(), new List<string>());
    }

    public static IPayBandThresholdStore NewCachedPayBandThresholdStore(MemoryPayBandThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandThresholdStore(inner, PayBandThresholdCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PayBandThreshold> FindPayBandThresholds(IPayBandThresholdStore store, PayBandThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
