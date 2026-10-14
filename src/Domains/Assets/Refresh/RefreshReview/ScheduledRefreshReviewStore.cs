namespace Acme.Workforce.Assets.Refresh.RefreshReview;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled refresh reviews by code.</summary>
public interface IScheduledRefreshReviewStore
{
    ScheduledRefreshReview? Find(string code);
    void Save(ScheduledRefreshReview item);
    bool Remove(string code);
    List<ScheduledRefreshReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledRefreshReviewStore(Dictionary<string, ScheduledRefreshReview> items, List<string> order) : IScheduledRefreshReviewStore
{
    public Dictionary<string, ScheduledRefreshReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledRefreshReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledRefreshReview item)
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

    public List<ScheduledRefreshReview> List()
    {
        var outValue = new List<ScheduledRefreshReview>();
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
public sealed class CachedScheduledRefreshReviewStore(MemoryScheduledRefreshReviewStore inner, ScheduledRefreshReviewCache cache, int hits, int misses) : IScheduledRefreshReviewStore
{
    public MemoryScheduledRefreshReviewStore Inner { get; set; } = inner;
    public ScheduledRefreshReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledRefreshReview? Find(string code)
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

    public void Save(ScheduledRefreshReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledRefreshReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled refresh reviews. Empty fields match all.</summary>
public sealed record ScheduledRefreshReviewQuery(string Title, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledRefreshReview item)
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

    public List<ScheduledRefreshReview> Apply(List<ScheduledRefreshReview> items)
    {
        var matched = Collections.Filter(items, (ScheduledRefreshReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled refresh review store.</summary>
public static class ScheduledRefreshReviewStore
{
    public static IScheduledRefreshReviewStore NewMemoryScheduledRefreshReviewStore()
    {
        return new MemoryScheduledRefreshReviewStore(
            new Dictionary<string, ScheduledRefreshReview>(),
            new List<string>());
    }

    public static IScheduledRefreshReviewStore NewCachedScheduledRefreshReviewStore(MemoryScheduledRefreshReviewStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledRefreshReviewStore(
            inner,
            ScheduledRefreshReviewCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledRefreshReview> FindScheduledRefreshReviews(IScheduledRefreshReviewStore store, ScheduledRefreshReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
