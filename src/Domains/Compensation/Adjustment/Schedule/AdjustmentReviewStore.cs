namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment reviews by code.</summary>
public interface IAdjustmentReviewStore
{
    AdjustmentReview? Find(string code);
    void Save(AdjustmentReview item);
    bool Remove(string code);
    List<AdjustmentReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentReviewStore(Dictionary<string, AdjustmentReview> items, List<string> order) : IAdjustmentReviewStore
{
    public Dictionary<string, AdjustmentReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentReview item)
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

    public List<AdjustmentReview> List()
    {
        var outValue = new List<AdjustmentReview>();
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
public sealed class CachedAdjustmentReviewStore(MemoryAdjustmentReviewStore inner, AdjustmentReviewCache cache, int hits, int misses) : IAdjustmentReviewStore
{
    public MemoryAdjustmentReviewStore Inner { get; set; } = inner;
    public AdjustmentReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentReview? Find(string code)
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

    public void Save(AdjustmentReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment reviews. Empty fields match all.</summary>
public sealed record AdjustmentReviewQuery(string Country, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentReview item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdjustmentReview> Apply(List<AdjustmentReview> items)
    {
        var matched = Collections.Filter(items, (AdjustmentReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment review store.</summary>
public static class AdjustmentReviewStore
{
    public static IAdjustmentReviewStore NewMemoryAdjustmentReviewStore()
    {
        return new MemoryAdjustmentReviewStore(new Dictionary<string, AdjustmentReview>(), new List<string>());
    }

    public static IAdjustmentReviewStore NewCachedAdjustmentReviewStore(MemoryAdjustmentReviewStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentReviewStore(inner, AdjustmentReviewCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AdjustmentReview> FindAdjustmentReviews(IAdjustmentReviewStore store, AdjustmentReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
