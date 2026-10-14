namespace Acme.Workforce.Absence.Holiday.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday reviews by code.</summary>
public interface IHolidayReviewStore
{
    HolidayReview? Find(string code);
    void Save(HolidayReview item);
    bool Remove(string code);
    List<HolidayReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayReviewStore(Dictionary<string, HolidayReview> items, List<string> order) : IHolidayReviewStore
{
    public Dictionary<string, HolidayReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayReview item)
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

    public List<HolidayReview> List()
    {
        var outValue = new List<HolidayReview>();
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
public sealed class CachedHolidayReviewStore(MemoryHolidayReviewStore inner, HolidayReviewCache cache, int hits, int misses) : IHolidayReviewStore
{
    public MemoryHolidayReviewStore Inner { get; set; } = inner;
    public HolidayReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayReview? Find(string code)
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

    public void Save(HolidayReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday reviews. Empty fields match all.</summary>
public sealed record HolidayReviewQuery(string Title, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayReview item)
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

    public List<HolidayReview> Apply(List<HolidayReview> items)
    {
        var matched = Collections.Filter(items, (HolidayReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday review store.</summary>
public static class HolidayReviewStore
{
    public static IHolidayReviewStore NewMemoryHolidayReviewStore()
    {
        return new MemoryHolidayReviewStore(new Dictionary<string, HolidayReview>(), new List<string>());
    }

    public static IHolidayReviewStore NewCachedHolidayReviewStore(MemoryHolidayReviewStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayReviewStore(inner, HolidayReviewCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<HolidayReview> FindHolidayReviews(IHolidayReviewStore store, HolidayReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
