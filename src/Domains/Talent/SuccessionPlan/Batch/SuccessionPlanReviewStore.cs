namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan reviews by code.</summary>
public interface ISuccessionPlanReviewStore
{
    SuccessionPlanReview? Find(string code);
    void Save(SuccessionPlanReview item);
    bool Remove(string code);
    List<SuccessionPlanReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanReviewStore(Dictionary<string, SuccessionPlanReview> items, List<string> order) : ISuccessionPlanReviewStore
{
    public Dictionary<string, SuccessionPlanReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanReview item)
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

    public List<SuccessionPlanReview> List()
    {
        var outValue = new List<SuccessionPlanReview>();
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
public sealed class CachedSuccessionPlanReviewStore(MemorySuccessionPlanReviewStore inner, SuccessionPlanReviewCache cache, int hits, int misses) : ISuccessionPlanReviewStore
{
    public MemorySuccessionPlanReviewStore Inner { get; set; } = inner;
    public SuccessionPlanReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanReview? Find(string code)
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

    public void Save(SuccessionPlanReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan reviews. Empty fields match all.</summary>
public sealed record SuccessionPlanReviewQuery(string Title, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanReview item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SuccessionPlanReview> Apply(List<SuccessionPlanReview> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan review store.</summary>
public static class SuccessionPlanReviewStore
{
    public static ISuccessionPlanReviewStore NewMemorySuccessionPlanReviewStore()
    {
        return new MemorySuccessionPlanReviewStore(
            new Dictionary<string, SuccessionPlanReview>(),
            new List<string>());
    }

    public static ISuccessionPlanReviewStore NewCachedSuccessionPlanReviewStore(MemorySuccessionPlanReviewStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanReviewStore(inner, SuccessionPlanReviewCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SuccessionPlanReview> FindSuccessionPlanReviews(ISuccessionPlanReviewStore store, SuccessionPlanReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
