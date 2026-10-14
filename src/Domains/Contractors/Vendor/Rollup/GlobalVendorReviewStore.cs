namespace Acme.Workforce.Contractors.Vendor.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global vendor reviews by code.</summary>
public interface IGlobalVendorReviewStore
{
    GlobalVendorReview? Find(string code);
    void Save(GlobalVendorReview item);
    bool Remove(string code);
    List<GlobalVendorReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalVendorReviewStore(Dictionary<string, GlobalVendorReview> items, List<string> order) : IGlobalVendorReviewStore
{
    public Dictionary<string, GlobalVendorReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalVendorReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalVendorReview item)
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

    public List<GlobalVendorReview> List()
    {
        var outValue = new List<GlobalVendorReview>();
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
public sealed class CachedGlobalVendorReviewStore(MemoryGlobalVendorReviewStore inner, GlobalVendorReviewCache cache, int hits, int misses) : IGlobalVendorReviewStore
{
    public MemoryGlobalVendorReviewStore Inner { get; set; } = inner;
    public GlobalVendorReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalVendorReview? Find(string code)
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

    public void Save(GlobalVendorReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalVendorReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global vendor reviews. Empty fields match all.</summary>
public sealed record GlobalVendorReviewQuery(string Channel, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalVendorReview item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalVendorReview> Apply(List<GlobalVendorReview> items)
    {
        var matched = Collections.Filter(items, (GlobalVendorReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global vendor review store.</summary>
public static class GlobalVendorReviewStore
{
    public static IGlobalVendorReviewStore NewMemoryGlobalVendorReviewStore()
    {
        return new MemoryGlobalVendorReviewStore(new Dictionary<string, GlobalVendorReview>(), new List<string>());
    }

    public static IGlobalVendorReviewStore NewCachedGlobalVendorReviewStore(MemoryGlobalVendorReviewStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalVendorReviewStore(inner, GlobalVendorReviewCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<GlobalVendorReview> FindGlobalVendorReviews(IGlobalVendorReviewStore store, GlobalVendorReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
