namespace Acme.Workforce.Orgdesign.Span.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling span reviews by code.</summary>
public interface IRollingSpanReviewStore
{
    RollingSpanReview? Find(string code);
    void Save(RollingSpanReview item);
    bool Remove(string code);
    List<RollingSpanReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingSpanReviewStore(Dictionary<string, RollingSpanReview> items, List<string> order) : IRollingSpanReviewStore
{
    public Dictionary<string, RollingSpanReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingSpanReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingSpanReview item)
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

    public List<RollingSpanReview> List()
    {
        var outValue = new List<RollingSpanReview>();
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
public sealed class CachedRollingSpanReviewStore(MemoryRollingSpanReviewStore inner, RollingSpanReviewCache cache, int hits, int misses) : IRollingSpanReviewStore
{
    public MemoryRollingSpanReviewStore Inner { get; set; } = inner;
    public RollingSpanReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingSpanReview? Find(string code)
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

    public void Save(RollingSpanReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingSpanReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling span reviews. Empty fields match all.</summary>
public sealed record RollingSpanReviewQuery(string Team, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(RollingSpanReview item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingSpanReview> Apply(List<RollingSpanReview> items)
    {
        var matched = Collections.Filter(items, (RollingSpanReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling span review store.</summary>
public static class RollingSpanReviewStore
{
    public static IRollingSpanReviewStore NewMemoryRollingSpanReviewStore()
    {
        return new MemoryRollingSpanReviewStore(new Dictionary<string, RollingSpanReview>(), new List<string>());
    }

    public static IRollingSpanReviewStore NewCachedRollingSpanReviewStore(MemoryRollingSpanReviewStore inner, int ttl, IClock clock)
    {
        return new CachedRollingSpanReviewStore(inner, RollingSpanReviewCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RollingSpanReview> FindRollingSpanReviews(IRollingSpanReviewStore store, RollingSpanReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
