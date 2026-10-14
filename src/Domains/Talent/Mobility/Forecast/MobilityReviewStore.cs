namespace Acme.Workforce.Talent.Mobility.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility reviews by code.</summary>
public interface IMobilityReviewStore
{
    MobilityReview? Find(string code);
    void Save(MobilityReview item);
    bool Remove(string code);
    List<MobilityReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilityReviewStore(Dictionary<string, MobilityReview> items, List<string> order) : IMobilityReviewStore
{
    public Dictionary<string, MobilityReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilityReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilityReview item)
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

    public List<MobilityReview> List()
    {
        var outValue = new List<MobilityReview>();
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
public sealed class CachedMobilityReviewStore(MemoryMobilityReviewStore inner, MobilityReviewCache cache, int hits, int misses) : IMobilityReviewStore
{
    public MemoryMobilityReviewStore Inner { get; set; } = inner;
    public MobilityReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilityReview? Find(string code)
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

    public void Save(MobilityReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilityReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility reviews. Empty fields match all.</summary>
public sealed record MobilityReviewQuery(string Source, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(MobilityReview item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MobilityReview> Apply(List<MobilityReview> items)
    {
        var matched = Collections.Filter(items, (MobilityReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility review store.</summary>
public static class MobilityReviewStore
{
    public static IMobilityReviewStore NewMemoryMobilityReviewStore()
    {
        return new MemoryMobilityReviewStore(new Dictionary<string, MobilityReview>(), new List<string>());
    }

    public static IMobilityReviewStore NewCachedMobilityReviewStore(MemoryMobilityReviewStore inner, int ttl, IClock clock)
    {
        return new CachedMobilityReviewStore(inner, MobilityReviewCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<MobilityReview> FindMobilityReviews(IMobilityReviewStore store, MobilityReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
