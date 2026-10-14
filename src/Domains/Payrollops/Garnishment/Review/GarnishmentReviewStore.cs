namespace Acme.Workforce.Payrollops.Garnishment.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment reviews by code.</summary>
public interface IGarnishmentReviewStore
{
    GarnishmentReview? Find(string code);
    void Save(GarnishmentReview item);
    bool Remove(string code);
    List<GarnishmentReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentReviewStore(Dictionary<string, GarnishmentReview> items, List<string> order) : IGarnishmentReviewStore
{
    public Dictionary<string, GarnishmentReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentReview item)
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

    public List<GarnishmentReview> List()
    {
        var outValue = new List<GarnishmentReview>();
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
public sealed class CachedGarnishmentReviewStore(MemoryGarnishmentReviewStore inner, GarnishmentReviewCache cache, int hits, int misses) : IGarnishmentReviewStore
{
    public MemoryGarnishmentReviewStore Inner { get; set; } = inner;
    public GarnishmentReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentReview? Find(string code)
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

    public void Save(GarnishmentReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment reviews. Empty fields match all.</summary>
public sealed record GarnishmentReviewQuery(string Channel, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentReview item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GarnishmentReview> Apply(List<GarnishmentReview> items)
    {
        var matched = Collections.Filter(items, (GarnishmentReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment review store.</summary>
public static class GarnishmentReviewStore
{
    public static IGarnishmentReviewStore NewMemoryGarnishmentReviewStore()
    {
        return new MemoryGarnishmentReviewStore(new Dictionary<string, GarnishmentReview>(), new List<string>());
    }

    public static IGarnishmentReviewStore NewCachedGarnishmentReviewStore(MemoryGarnishmentReviewStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentReviewStore(inner, GarnishmentReviewCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<GarnishmentReview> FindGarnishmentReviews(IGarnishmentReviewStore store, GarnishmentReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
