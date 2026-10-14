namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary offboarding reviews by code.</summary>
public interface IPrimaryOffboardingReviewStore
{
    PrimaryOffboardingReview? Find(string code);
    void Save(PrimaryOffboardingReview item);
    bool Remove(string code);
    List<PrimaryOffboardingReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryOffboardingReviewStore(Dictionary<string, PrimaryOffboardingReview> items, List<string> order) : IPrimaryOffboardingReviewStore
{
    public Dictionary<string, PrimaryOffboardingReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryOffboardingReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryOffboardingReview item)
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

    public List<PrimaryOffboardingReview> List()
    {
        var outValue = new List<PrimaryOffboardingReview>();
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
public sealed class CachedPrimaryOffboardingReviewStore(MemoryPrimaryOffboardingReviewStore inner, PrimaryOffboardingReviewCache cache, int hits, int misses) : IPrimaryOffboardingReviewStore
{
    public MemoryPrimaryOffboardingReviewStore Inner { get; set; } = inner;
    public PrimaryOffboardingReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryOffboardingReview? Find(string code)
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

    public void Save(PrimaryOffboardingReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryOffboardingReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary offboarding reviews. Empty fields match all.</summary>
public sealed record PrimaryOffboardingReviewQuery(string Segment, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryOffboardingReview item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryOffboardingReview> Apply(List<PrimaryOffboardingReview> items)
    {
        var matched = Collections.Filter(items, (PrimaryOffboardingReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary offboarding review store.</summary>
public static class PrimaryOffboardingReviewStore
{
    public static IPrimaryOffboardingReviewStore NewMemoryPrimaryOffboardingReviewStore()
    {
        return new MemoryPrimaryOffboardingReviewStore(
            new Dictionary<string, PrimaryOffboardingReview>(),
            new List<string>());
    }

    public static IPrimaryOffboardingReviewStore NewCachedPrimaryOffboardingReviewStore(MemoryPrimaryOffboardingReviewStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryOffboardingReviewStore(
            inner,
            PrimaryOffboardingReviewCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryOffboardingReview> FindPrimaryOffboardingReviews(IPrimaryOffboardingReviewStore store, PrimaryOffboardingReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
