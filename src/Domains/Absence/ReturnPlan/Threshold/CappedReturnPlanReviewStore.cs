namespace Acme.Workforce.Absence.ReturnPlan.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped return plan reviews by code.</summary>
public interface ICappedReturnPlanReviewStore
{
    CappedReturnPlanReview? Find(string code);
    void Save(CappedReturnPlanReview item);
    bool Remove(string code);
    List<CappedReturnPlanReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedReturnPlanReviewStore(Dictionary<string, CappedReturnPlanReview> items, List<string> order) : ICappedReturnPlanReviewStore
{
    public Dictionary<string, CappedReturnPlanReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedReturnPlanReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedReturnPlanReview item)
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

    public List<CappedReturnPlanReview> List()
    {
        var outValue = new List<CappedReturnPlanReview>();
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
public sealed class CachedCappedReturnPlanReviewStore(MemoryCappedReturnPlanReviewStore inner, CappedReturnPlanReviewCache cache, int hits, int misses) : ICappedReturnPlanReviewStore
{
    public MemoryCappedReturnPlanReviewStore Inner { get; set; } = inner;
    public CappedReturnPlanReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedReturnPlanReview? Find(string code)
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

    public void Save(CappedReturnPlanReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedReturnPlanReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped return plan reviews. Empty fields match all.</summary>
public sealed record CappedReturnPlanReviewQuery(string Title, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(CappedReturnPlanReview item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedReturnPlanReview> Apply(List<CappedReturnPlanReview> items)
    {
        var matched = Collections.Filter(items, (CappedReturnPlanReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped return plan review store.</summary>
public static class CappedReturnPlanReviewStore
{
    public static ICappedReturnPlanReviewStore NewMemoryCappedReturnPlanReviewStore()
    {
        return new MemoryCappedReturnPlanReviewStore(
            new Dictionary<string, CappedReturnPlanReview>(),
            new List<string>());
    }

    public static ICappedReturnPlanReviewStore NewCachedCappedReturnPlanReviewStore(MemoryCappedReturnPlanReviewStore inner, int ttl, IClock clock)
    {
        return new CachedCappedReturnPlanReviewStore(
            inner,
            CappedReturnPlanReviewCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<CappedReturnPlanReview> FindCappedReturnPlanReviews(ICappedReturnPlanReviewStore store, CappedReturnPlanReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
