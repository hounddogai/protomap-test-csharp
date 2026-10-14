namespace Acme.Workforce.Contractors.Rate.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate reviews by code.</summary>
public interface IRateReviewStore
{
    RateReview? Find(string code);
    void Save(RateReview item);
    bool Remove(string code);
    List<RateReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateReviewStore(Dictionary<string, RateReview> items, List<string> order) : IRateReviewStore
{
    public Dictionary<string, RateReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateReview item)
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

    public List<RateReview> List()
    {
        var outValue = new List<RateReview>();
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
public sealed class CachedRateReviewStore(MemoryRateReviewStore inner, RateReviewCache cache, int hits, int misses) : IRateReviewStore
{
    public MemoryRateReviewStore Inner { get; set; } = inner;
    public RateReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateReview? Find(string code)
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

    public void Save(RateReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate reviews. Empty fields match all.</summary>
public sealed record RateReviewQuery(string Name, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(RateReview item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<RateReview> Apply(List<RateReview> items)
    {
        var matched = Collections.Filter(items, (RateReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate review store.</summary>
public static class RateReviewStore
{
    public static IRateReviewStore NewMemoryRateReviewStore()
    {
        return new MemoryRateReviewStore(new Dictionary<string, RateReview>(), new List<string>());
    }

    public static IRateReviewStore NewCachedRateReviewStore(MemoryRateReviewStore inner, int ttl, IClock clock)
    {
        return new CachedRateReviewStore(inner, RateReviewCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RateReview> FindRateReviews(IRateReviewStore store, RateReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
