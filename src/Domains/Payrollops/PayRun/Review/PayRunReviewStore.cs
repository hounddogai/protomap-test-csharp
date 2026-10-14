namespace Acme.Workforce.Payrollops.PayRun.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run reviews by code.</summary>
public interface IPayRunReviewStore
{
    PayRunReview? Find(string code);
    void Save(PayRunReview item);
    bool Remove(string code);
    List<PayRunReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunReviewStore(Dictionary<string, PayRunReview> items, List<string> order) : IPayRunReviewStore
{
    public Dictionary<string, PayRunReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunReview item)
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

    public List<PayRunReview> List()
    {
        var outValue = new List<PayRunReview>();
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
public sealed class CachedPayRunReviewStore(MemoryPayRunReviewStore inner, PayRunReviewCache cache, int hits, int misses) : IPayRunReviewStore
{
    public MemoryPayRunReviewStore Inner { get; set; } = inner;
    public PayRunReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunReview? Find(string code)
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

    public void Save(PayRunReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run reviews. Empty fields match all.</summary>
public sealed record PayRunReviewQuery(string Title, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunReview item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayRunReview> Apply(List<PayRunReview> items)
    {
        var matched = Collections.Filter(items, (PayRunReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run review store.</summary>
public static class PayRunReviewStore
{
    public static IPayRunReviewStore NewMemoryPayRunReviewStore()
    {
        return new MemoryPayRunReviewStore(new Dictionary<string, PayRunReview>(), new List<string>());
    }

    public static IPayRunReviewStore NewCachedPayRunReviewStore(MemoryPayRunReviewStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunReviewStore(inner, PayRunReviewCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PayRunReview> FindPayRunReviews(IPayRunReviewStore store, PayRunReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
