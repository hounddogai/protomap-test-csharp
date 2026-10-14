namespace Acme.Workforce.Assets.Refresh.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh reviews by code.</summary>
public interface IRefreshReviewStore
{
    RefreshReview? Find(string code);
    void Save(RefreshReview item);
    bool Remove(string code);
    List<RefreshReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshReviewStore(Dictionary<string, RefreshReview> items, List<string> order) : IRefreshReviewStore
{
    public Dictionary<string, RefreshReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshReview item)
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

    public List<RefreshReview> List()
    {
        var outValue = new List<RefreshReview>();
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
public sealed class CachedRefreshReviewStore(MemoryRefreshReviewStore inner, RefreshReviewCache cache, int hits, int misses) : IRefreshReviewStore
{
    public MemoryRefreshReviewStore Inner { get; set; } = inner;
    public RefreshReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshReview? Find(string code)
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

    public void Save(RefreshReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh reviews. Empty fields match all.</summary>
public sealed record RefreshReviewQuery(string Segment, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshReview item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RefreshReview> Apply(List<RefreshReview> items)
    {
        var matched = Collections.Filter(items, (RefreshReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh review store.</summary>
public static class RefreshReviewStore
{
    public static IRefreshReviewStore NewMemoryRefreshReviewStore()
    {
        return new MemoryRefreshReviewStore(new Dictionary<string, RefreshReview>(), new List<string>());
    }

    public static IRefreshReviewStore NewCachedRefreshReviewStore(MemoryRefreshReviewStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshReviewStore(inner, RefreshReviewCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RefreshReview> FindRefreshReviews(IRefreshReviewStore store, RefreshReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
