namespace Acme.Workforce.Engagement.ActionPlan.PlanSegment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan reviews by code.</summary>
public interface IActionPlanReviewStore
{
    ActionPlanReview? Find(string code);
    void Save(ActionPlanReview item);
    bool Remove(string code);
    List<ActionPlanReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanReviewStore(Dictionary<string, ActionPlanReview> items, List<string> order) : IActionPlanReviewStore
{
    public Dictionary<string, ActionPlanReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanReview item)
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

    public List<ActionPlanReview> List()
    {
        var outValue = new List<ActionPlanReview>();
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
public sealed class CachedActionPlanReviewStore(MemoryActionPlanReviewStore inner, ActionPlanReviewCache cache, int hits, int misses) : IActionPlanReviewStore
{
    public MemoryActionPlanReviewStore Inner { get; set; } = inner;
    public ActionPlanReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanReview? Find(string code)
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

    public void Save(ActionPlanReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan reviews. Empty fields match all.</summary>
public sealed record ActionPlanReviewQuery(string Category, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanReview item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<ActionPlanReview> Apply(List<ActionPlanReview> items)
    {
        var matched = Collections.Filter(items, (ActionPlanReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan review store.</summary>
public static class ActionPlanReviewStore
{
    public static IActionPlanReviewStore NewMemoryActionPlanReviewStore()
    {
        return new MemoryActionPlanReviewStore(new Dictionary<string, ActionPlanReview>(), new List<string>());
    }

    public static IActionPlanReviewStore NewCachedActionPlanReviewStore(MemoryActionPlanReviewStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanReviewStore(inner, ActionPlanReviewCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ActionPlanReview> FindActionPlanReviews(IActionPlanReviewStore store, ActionPlanReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
