namespace Acme.Workforce.Workplace.Occupancy.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy reviews by code.</summary>
public interface IOccupancyReviewStore
{
    OccupancyReview? Find(string code);
    void Save(OccupancyReview item);
    bool Remove(string code);
    List<OccupancyReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyReviewStore(Dictionary<string, OccupancyReview> items, List<string> order) : IOccupancyReviewStore
{
    public Dictionary<string, OccupancyReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyReview item)
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

    public List<OccupancyReview> List()
    {
        var outValue = new List<OccupancyReview>();
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
public sealed class CachedOccupancyReviewStore(MemoryOccupancyReviewStore inner, OccupancyReviewCache cache, int hits, int misses) : IOccupancyReviewStore
{
    public MemoryOccupancyReviewStore Inner { get; set; } = inner;
    public OccupancyReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyReview? Find(string code)
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

    public void Save(OccupancyReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy reviews. Empty fields match all.</summary>
public sealed record OccupancyReviewQuery(string Category, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyReview item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<OccupancyReview> Apply(List<OccupancyReview> items)
    {
        var matched = Collections.Filter(items, (OccupancyReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy review store.</summary>
public static class OccupancyReviewStore
{
    public static IOccupancyReviewStore NewMemoryOccupancyReviewStore()
    {
        return new MemoryOccupancyReviewStore(new Dictionary<string, OccupancyReview>(), new List<string>());
    }

    public static IOccupancyReviewStore NewCachedOccupancyReviewStore(MemoryOccupancyReviewStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyReviewStore(inner, OccupancyReviewCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<OccupancyReview> FindOccupancyReviews(IOccupancyReviewStore store, OccupancyReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
