namespace Acme.Workforce.Compensation.MeritCycle.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle reviews by code.</summary>
public interface IMeritCycleReviewStore
{
    MeritCycleReview? Find(string code);
    void Save(MeritCycleReview item);
    bool Remove(string code);
    List<MeritCycleReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleReviewStore(Dictionary<string, MeritCycleReview> items, List<string> order) : IMeritCycleReviewStore
{
    public Dictionary<string, MeritCycleReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleReview item)
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

    public List<MeritCycleReview> List()
    {
        var outValue = new List<MeritCycleReview>();
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
public sealed class CachedMeritCycleReviewStore(MemoryMeritCycleReviewStore inner, MeritCycleReviewCache cache, int hits, int misses) : IMeritCycleReviewStore
{
    public MemoryMeritCycleReviewStore Inner { get; set; } = inner;
    public MeritCycleReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleReview? Find(string code)
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

    public void Save(MeritCycleReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle reviews. Empty fields match all.</summary>
public sealed record MeritCycleReviewQuery(string Title, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleReview item)
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

    public List<MeritCycleReview> Apply(List<MeritCycleReview> items)
    {
        var matched = Collections.Filter(items, (MeritCycleReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle review store.</summary>
public static class MeritCycleReviewStore
{
    public static IMeritCycleReviewStore NewMemoryMeritCycleReviewStore()
    {
        return new MemoryMeritCycleReviewStore(new Dictionary<string, MeritCycleReview>(), new List<string>());
    }

    public static IMeritCycleReviewStore NewCachedMeritCycleReviewStore(MemoryMeritCycleReviewStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleReviewStore(inner, MeritCycleReviewCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<MeritCycleReview> FindMeritCycleReviews(IMeritCycleReviewStore store, MeritCycleReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
