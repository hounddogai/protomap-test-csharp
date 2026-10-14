namespace Acme.Workforce.Compliance.Certification.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps certification reviews by code.</summary>
public interface ICertificationReviewStore
{
    CertificationReview? Find(string code);
    void Save(CertificationReview item);
    bool Remove(string code);
    List<CertificationReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCertificationReviewStore(Dictionary<string, CertificationReview> items, List<string> order) : ICertificationReviewStore
{
    public Dictionary<string, CertificationReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CertificationReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CertificationReview item)
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

    public List<CertificationReview> List()
    {
        var outValue = new List<CertificationReview>();
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
public sealed class CachedCertificationReviewStore(MemoryCertificationReviewStore inner, CertificationReviewCache cache, int hits, int misses) : ICertificationReviewStore
{
    public MemoryCertificationReviewStore Inner { get; set; } = inner;
    public CertificationReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CertificationReview? Find(string code)
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

    public void Save(CertificationReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CertificationReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects certification reviews. Empty fields match all.</summary>
public sealed record CertificationReviewQuery(string Title, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(CertificationReview item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<CertificationReview> Apply(List<CertificationReview> items)
    {
        var matched = Collections.Filter(items, (CertificationReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of certification review store.</summary>
public static class CertificationReviewStore
{
    public static ICertificationReviewStore NewMemoryCertificationReviewStore()
    {
        return new MemoryCertificationReviewStore(new Dictionary<string, CertificationReview>(), new List<string>());
    }

    public static ICertificationReviewStore NewCachedCertificationReviewStore(MemoryCertificationReviewStore inner, int ttl, IClock clock)
    {
        return new CachedCertificationReviewStore(inner, CertificationReviewCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CertificationReview> FindCertificationReviews(ICertificationReviewStore store, CertificationReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
