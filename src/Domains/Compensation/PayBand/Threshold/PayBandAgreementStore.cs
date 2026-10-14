namespace Acme.Workforce.Compensation.PayBand.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band agreements by code.</summary>
public interface IPayBandAgreementStore
{
    PayBandAgreement? Find(string code);
    void Save(PayBandAgreement item);
    bool Remove(string code);
    List<PayBandAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandAgreementStore(Dictionary<string, PayBandAgreement> items, List<string> order) : IPayBandAgreementStore
{
    public Dictionary<string, PayBandAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandAgreement item)
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

    public List<PayBandAgreement> List()
    {
        var outValue = new List<PayBandAgreement>();
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
public sealed class CachedPayBandAgreementStore(MemoryPayBandAgreementStore inner, PayBandAgreementCache cache, int hits, int misses) : IPayBandAgreementStore
{
    public MemoryPayBandAgreementStore Inner { get; set; } = inner;
    public PayBandAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandAgreement? Find(string code)
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

    public void Save(PayBandAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band agreements. Empty fields match all.</summary>
public sealed record PayBandAgreementQuery(string Locale, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandAgreement item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<PayBandAgreement> Apply(List<PayBandAgreement> items)
    {
        var matched = Collections.Filter(items, (PayBandAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band agreement store.</summary>
public static class PayBandAgreementStore
{
    public static IPayBandAgreementStore NewMemoryPayBandAgreementStore()
    {
        return new MemoryPayBandAgreementStore(new Dictionary<string, PayBandAgreement>(), new List<string>());
    }

    public static IPayBandAgreementStore NewCachedPayBandAgreementStore(MemoryPayBandAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandAgreementStore(inner, PayBandAgreementCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PayBandAgreement> FindPayBandAgreements(IPayBandAgreementStore store, PayBandAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
