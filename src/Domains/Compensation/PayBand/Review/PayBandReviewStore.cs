namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band reviews by code.</summary>
public interface IPayBandReviewStore
{
    PayBandReview? Find(string code);
    void Save(PayBandReview item);
    bool Remove(string code);
    List<PayBandReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandReviewStore(Dictionary<string, PayBandReview> items, List<string> order) : IPayBandReviewStore
{
    public Dictionary<string, PayBandReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandReview item)
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

    public List<PayBandReview> List()
    {
        var outValue = new List<PayBandReview>();
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
public sealed class CachedPayBandReviewStore(MemoryPayBandReviewStore inner, PayBandReviewCache cache, int hits, int misses) : IPayBandReviewStore
{
    public MemoryPayBandReviewStore Inner { get; set; } = inner;
    public PayBandReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandReview? Find(string code)
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

    public void Save(PayBandReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band reviews. Empty fields match all.</summary>
public sealed record PayBandReviewQuery(string Name, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandReview item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandReview> Apply(List<PayBandReview> items)
    {
        var matched = Collections.Filter(items, (PayBandReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band review store.</summary>
public static class PayBandReviewStore
{
    public static IPayBandReviewStore NewMemoryPayBandReviewStore()
    {
        return new MemoryPayBandReviewStore(new Dictionary<string, PayBandReview>(), new List<string>());
    }

    public static IPayBandReviewStore NewCachedPayBandReviewStore(MemoryPayBandReviewStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandReviewStore(inner, PayBandReviewCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PayBandReview> FindPayBandReviews(IPayBandReviewStore store, PayBandReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
