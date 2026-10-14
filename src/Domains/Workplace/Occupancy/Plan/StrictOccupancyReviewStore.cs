namespace Acme.Workforce.Workplace.Occupancy.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict occupancy reviews by code.</summary>
public interface IStrictOccupancyReviewStore
{
    StrictOccupancyReview? Find(string code);
    void Save(StrictOccupancyReview item);
    bool Remove(string code);
    List<StrictOccupancyReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictOccupancyReviewStore(Dictionary<string, StrictOccupancyReview> items, List<string> order) : IStrictOccupancyReviewStore
{
    public Dictionary<string, StrictOccupancyReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictOccupancyReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictOccupancyReview item)
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

    public List<StrictOccupancyReview> List()
    {
        var outValue = new List<StrictOccupancyReview>();
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
public sealed class CachedStrictOccupancyReviewStore(MemoryStrictOccupancyReviewStore inner, StrictOccupancyReviewCache cache, int hits, int misses) : IStrictOccupancyReviewStore
{
    public MemoryStrictOccupancyReviewStore Inner { get; set; } = inner;
    public StrictOccupancyReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictOccupancyReview? Find(string code)
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

    public void Save(StrictOccupancyReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictOccupancyReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict occupancy reviews. Empty fields match all.</summary>
public sealed record StrictOccupancyReviewQuery(string Title, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(StrictOccupancyReview item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictOccupancyReview> Apply(List<StrictOccupancyReview> items)
    {
        var matched = Collections.Filter(items, (StrictOccupancyReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict occupancy review store.</summary>
public static class StrictOccupancyReviewStore
{
    public static IStrictOccupancyReviewStore NewMemoryStrictOccupancyReviewStore()
    {
        return new MemoryStrictOccupancyReviewStore(
            new Dictionary<string, StrictOccupancyReview>(),
            new List<string>());
    }

    public static IStrictOccupancyReviewStore NewCachedStrictOccupancyReviewStore(MemoryStrictOccupancyReviewStore inner, int ttl, IClock clock)
    {
        return new CachedStrictOccupancyReviewStore(inner, StrictOccupancyReviewCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<StrictOccupancyReview> FindStrictOccupancyReviews(IStrictOccupancyReviewStore store, StrictOccupancyReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
