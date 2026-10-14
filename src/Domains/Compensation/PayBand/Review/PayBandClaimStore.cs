namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band claims by code.</summary>
public interface IPayBandClaimStore
{
    PayBandClaim? Find(string code);
    void Save(PayBandClaim item);
    bool Remove(string code);
    List<PayBandClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandClaimStore(Dictionary<string, PayBandClaim> items, List<string> order) : IPayBandClaimStore
{
    public Dictionary<string, PayBandClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandClaim item)
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

    public List<PayBandClaim> List()
    {
        var outValue = new List<PayBandClaim>();
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
public sealed class CachedPayBandClaimStore(MemoryPayBandClaimStore inner, PayBandClaimCache cache, int hits, int misses) : IPayBandClaimStore
{
    public MemoryPayBandClaimStore Inner { get; set; } = inner;
    public PayBandClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandClaim? Find(string code)
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

    public void Save(PayBandClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band claims. Empty fields match all.</summary>
public sealed record PayBandClaimQuery(string Label, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandClaim item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandClaim> Apply(List<PayBandClaim> items)
    {
        var matched = Collections.Filter(items, (PayBandClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band claim store.</summary>
public static class PayBandClaimStore
{
    public static IPayBandClaimStore NewMemoryPayBandClaimStore()
    {
        return new MemoryPayBandClaimStore(new Dictionary<string, PayBandClaim>(), new List<string>());
    }

    public static IPayBandClaimStore NewCachedPayBandClaimStore(MemoryPayBandClaimStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandClaimStore(inner, PayBandClaimCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PayBandClaim> FindPayBandClaims(IPayBandClaimStore store, PayBandClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
