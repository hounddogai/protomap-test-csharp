namespace Acme.Workforce.Contractors.Rate.RateReview;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global rate reviews by code.</summary>
public interface IGlobalRateReviewStore
{
    GlobalRateReview? Find(string code);
    void Save(GlobalRateReview item);
    bool Remove(string code);
    List<GlobalRateReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalRateReviewStore(Dictionary<string, GlobalRateReview> items, List<string> order) : IGlobalRateReviewStore
{
    public Dictionary<string, GlobalRateReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalRateReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalRateReview item)
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

    public List<GlobalRateReview> List()
    {
        var outValue = new List<GlobalRateReview>();
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
public sealed class CachedGlobalRateReviewStore(MemoryGlobalRateReviewStore inner, GlobalRateReviewCache cache, int hits, int misses) : IGlobalRateReviewStore
{
    public MemoryGlobalRateReviewStore Inner { get; set; } = inner;
    public GlobalRateReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalRateReview? Find(string code)
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

    public void Save(GlobalRateReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalRateReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global rate reviews. Empty fields match all.</summary>
public sealed record GlobalRateReviewQuery(string Country, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalRateReview item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalRateReview> Apply(List<GlobalRateReview> items)
    {
        var matched = Collections.Filter(items, (GlobalRateReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global rate review store.</summary>
public static class GlobalRateReviewStore
{
    public static IGlobalRateReviewStore NewMemoryGlobalRateReviewStore()
    {
        return new MemoryGlobalRateReviewStore(new Dictionary<string, GlobalRateReview>(), new List<string>());
    }

    public static IGlobalRateReviewStore NewCachedGlobalRateReviewStore(MemoryGlobalRateReviewStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalRateReviewStore(inner, GlobalRateReviewCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<GlobalRateReview> FindGlobalRateReviews(IGlobalRateReviewStore store, GlobalRateReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
