namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal leave request reviews by code.</summary>
public interface ISeasonalLeaveRequestReviewStore
{
    SeasonalLeaveRequestReview? Find(string code);
    void Save(SeasonalLeaveRequestReview item);
    bool Remove(string code);
    List<SeasonalLeaveRequestReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalLeaveRequestReviewStore(Dictionary<string, SeasonalLeaveRequestReview> items, List<string> order) : ISeasonalLeaveRequestReviewStore
{
    public Dictionary<string, SeasonalLeaveRequestReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalLeaveRequestReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalLeaveRequestReview item)
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

    public List<SeasonalLeaveRequestReview> List()
    {
        var outValue = new List<SeasonalLeaveRequestReview>();
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
public sealed class CachedSeasonalLeaveRequestReviewStore(MemorySeasonalLeaveRequestReviewStore inner, SeasonalLeaveRequestReviewCache cache, int hits, int misses) : ISeasonalLeaveRequestReviewStore
{
    public MemorySeasonalLeaveRequestReviewStore Inner { get; set; } = inner;
    public SeasonalLeaveRequestReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalLeaveRequestReview? Find(string code)
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

    public void Save(SeasonalLeaveRequestReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalLeaveRequestReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal leave request reviews. Empty fields match all.</summary>
public sealed record SeasonalLeaveRequestReviewQuery(string Title, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalLeaveRequestReview item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalLeaveRequestReview> Apply(List<SeasonalLeaveRequestReview> items)
    {
        var matched = Collections.Filter(items, (SeasonalLeaveRequestReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal leave request review store.</summary>
public static class SeasonalLeaveRequestReviewStore
{
    public static ISeasonalLeaveRequestReviewStore NewMemorySeasonalLeaveRequestReviewStore()
    {
        return new MemorySeasonalLeaveRequestReviewStore(
            new Dictionary<string, SeasonalLeaveRequestReview>(),
            new List<string>());
    }

    public static ISeasonalLeaveRequestReviewStore NewCachedSeasonalLeaveRequestReviewStore(MemorySeasonalLeaveRequestReviewStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalLeaveRequestReviewStore(
            inner,
            SeasonalLeaveRequestReviewCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalLeaveRequestReview> FindSeasonalLeaveRequestReviews(ISeasonalLeaveRequestReviewStore store, SeasonalLeaveRequestReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
