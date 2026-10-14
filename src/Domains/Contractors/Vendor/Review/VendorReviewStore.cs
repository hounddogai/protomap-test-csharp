namespace Acme.Workforce.Contractors.Vendor.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor reviews by code.</summary>
public interface IVendorReviewStore
{
    VendorReview? Find(string code);
    void Save(VendorReview item);
    bool Remove(string code);
    List<VendorReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorReviewStore(Dictionary<string, VendorReview> items, List<string> order) : IVendorReviewStore
{
    public Dictionary<string, VendorReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorReview item)
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

    public List<VendorReview> List()
    {
        var outValue = new List<VendorReview>();
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
public sealed class CachedVendorReviewStore(MemoryVendorReviewStore inner, VendorReviewCache cache, int hits, int misses) : IVendorReviewStore
{
    public MemoryVendorReviewStore Inner { get; set; } = inner;
    public VendorReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorReview? Find(string code)
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

    public void Save(VendorReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor reviews. Empty fields match all.</summary>
public sealed record VendorReviewQuery(string Title, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(VendorReview item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VendorReview> Apply(List<VendorReview> items)
    {
        var matched = Collections.Filter(items, (VendorReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor review store.</summary>
public static class VendorReviewStore
{
    public static IVendorReviewStore NewMemoryVendorReviewStore()
    {
        return new MemoryVendorReviewStore(new Dictionary<string, VendorReview>(), new List<string>());
    }

    public static IVendorReviewStore NewCachedVendorReviewStore(MemoryVendorReviewStore inner, int ttl, IClock clock)
    {
        return new CachedVendorReviewStore(inner, VendorReviewCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<VendorReview> FindVendorReviews(IVendorReviewStore store, VendorReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
