namespace Acme.Workforce.Engagement.TownHall.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback town hall reviews by code.</summary>
public interface IFallbackTownHallReviewStore
{
    FallbackTownHallReview? Find(string code);
    void Save(FallbackTownHallReview item);
    bool Remove(string code);
    List<FallbackTownHallReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackTownHallReviewStore(Dictionary<string, FallbackTownHallReview> items, List<string> order) : IFallbackTownHallReviewStore
{
    public Dictionary<string, FallbackTownHallReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackTownHallReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackTownHallReview item)
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

    public List<FallbackTownHallReview> List()
    {
        var outValue = new List<FallbackTownHallReview>();
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
public sealed class CachedFallbackTownHallReviewStore(MemoryFallbackTownHallReviewStore inner, FallbackTownHallReviewCache cache, int hits, int misses) : IFallbackTownHallReviewStore
{
    public MemoryFallbackTownHallReviewStore Inner { get; set; } = inner;
    public FallbackTownHallReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackTownHallReview? Find(string code)
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

    public void Save(FallbackTownHallReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackTownHallReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback town hall reviews. Empty fields match all.</summary>
public sealed record FallbackTownHallReviewQuery(string Label, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackTownHallReview item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackTownHallReview> Apply(List<FallbackTownHallReview> items)
    {
        var matched = Collections.Filter(items, (FallbackTownHallReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback town hall review store.</summary>
public static class FallbackTownHallReviewStore
{
    public static IFallbackTownHallReviewStore NewMemoryFallbackTownHallReviewStore()
    {
        return new MemoryFallbackTownHallReviewStore(
            new Dictionary<string, FallbackTownHallReview>(),
            new List<string>());
    }

    public static IFallbackTownHallReviewStore NewCachedFallbackTownHallReviewStore(MemoryFallbackTownHallReviewStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackTownHallReviewStore(
            inner,
            FallbackTownHallReviewCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<FallbackTownHallReview> FindFallbackTownHallReviews(IFallbackTownHallReviewStore store, FallbackTownHallReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
