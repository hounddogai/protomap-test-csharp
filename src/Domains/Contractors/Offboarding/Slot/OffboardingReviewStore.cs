namespace Acme.Workforce.Contractors.Offboarding.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding reviews by code.</summary>
public interface IOffboardingReviewStore
{
    OffboardingReview? Find(string code);
    void Save(OffboardingReview item);
    bool Remove(string code);
    List<OffboardingReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingReviewStore(Dictionary<string, OffboardingReview> items, List<string> order) : IOffboardingReviewStore
{
    public Dictionary<string, OffboardingReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingReview item)
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

    public List<OffboardingReview> List()
    {
        var outValue = new List<OffboardingReview>();
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
public sealed class CachedOffboardingReviewStore(MemoryOffboardingReviewStore inner, OffboardingReviewCache cache, int hits, int misses) : IOffboardingReviewStore
{
    public MemoryOffboardingReviewStore Inner { get; set; } = inner;
    public OffboardingReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingReview? Find(string code)
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

    public void Save(OffboardingReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding reviews. Empty fields match all.</summary>
public sealed record OffboardingReviewQuery(string Country, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingReview item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OffboardingReview> Apply(List<OffboardingReview> items)
    {
        var matched = Collections.Filter(items, (OffboardingReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding review store.</summary>
public static class OffboardingReviewStore
{
    public static IOffboardingReviewStore NewMemoryOffboardingReviewStore()
    {
        return new MemoryOffboardingReviewStore(new Dictionary<string, OffboardingReview>(), new List<string>());
    }

    public static IOffboardingReviewStore NewCachedOffboardingReviewStore(MemoryOffboardingReviewStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingReviewStore(inner, OffboardingReviewCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<OffboardingReview> FindOffboardingReviews(IOffboardingReviewStore store, OffboardingReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
