namespace Acme.Workforce.Compensation.Adjustment.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling adjustment reviews by code.</summary>
public interface IRollingAdjustmentReviewStore
{
    RollingAdjustmentReview? Find(string code);
    void Save(RollingAdjustmentReview item);
    bool Remove(string code);
    List<RollingAdjustmentReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingAdjustmentReviewStore(Dictionary<string, RollingAdjustmentReview> items, List<string> order) : IRollingAdjustmentReviewStore
{
    public Dictionary<string, RollingAdjustmentReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingAdjustmentReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingAdjustmentReview item)
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

    public List<RollingAdjustmentReview> List()
    {
        var outValue = new List<RollingAdjustmentReview>();
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
public sealed class CachedRollingAdjustmentReviewStore(MemoryRollingAdjustmentReviewStore inner, RollingAdjustmentReviewCache cache, int hits, int misses) : IRollingAdjustmentReviewStore
{
    public MemoryRollingAdjustmentReviewStore Inner { get; set; } = inner;
    public RollingAdjustmentReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingAdjustmentReview? Find(string code)
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

    public void Save(RollingAdjustmentReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingAdjustmentReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling adjustment reviews. Empty fields match all.</summary>
public sealed record RollingAdjustmentReviewQuery(string Team, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(RollingAdjustmentReview item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingAdjustmentReview> Apply(List<RollingAdjustmentReview> items)
    {
        var matched = Collections.Filter(items, (RollingAdjustmentReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling adjustment review store.</summary>
public static class RollingAdjustmentReviewStore
{
    public static IRollingAdjustmentReviewStore NewMemoryRollingAdjustmentReviewStore()
    {
        return new MemoryRollingAdjustmentReviewStore(
            new Dictionary<string, RollingAdjustmentReview>(),
            new List<string>());
    }

    public static IRollingAdjustmentReviewStore NewCachedRollingAdjustmentReviewStore(MemoryRollingAdjustmentReviewStore inner, int ttl, IClock clock)
    {
        return new CachedRollingAdjustmentReviewStore(
            inner,
            RollingAdjustmentReviewCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RollingAdjustmentReview> FindRollingAdjustmentReviews(IRollingAdjustmentReviewStore store, RollingAdjustmentReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
