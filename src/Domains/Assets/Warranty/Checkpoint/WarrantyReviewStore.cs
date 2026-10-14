namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty reviews by code.</summary>
public interface IWarrantyReviewStore
{
    WarrantyReview? Find(string code);
    void Save(WarrantyReview item);
    bool Remove(string code);
    List<WarrantyReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyReviewStore(Dictionary<string, WarrantyReview> items, List<string> order) : IWarrantyReviewStore
{
    public Dictionary<string, WarrantyReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyReview item)
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

    public List<WarrantyReview> List()
    {
        var outValue = new List<WarrantyReview>();
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
public sealed class CachedWarrantyReviewStore(MemoryWarrantyReviewStore inner, WarrantyReviewCache cache, int hits, int misses) : IWarrantyReviewStore
{
    public MemoryWarrantyReviewStore Inner { get; set; } = inner;
    public WarrantyReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyReview? Find(string code)
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

    public void Save(WarrantyReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty reviews. Empty fields match all.</summary>
public sealed record WarrantyReviewQuery(string Name, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyReview item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyReview> Apply(List<WarrantyReview> items)
    {
        var matched = Collections.Filter(items, (WarrantyReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty review store.</summary>
public static class WarrantyReviewStore
{
    public static IWarrantyReviewStore NewMemoryWarrantyReviewStore()
    {
        return new MemoryWarrantyReviewStore(new Dictionary<string, WarrantyReview>(), new List<string>());
    }

    public static IWarrantyReviewStore NewCachedWarrantyReviewStore(MemoryWarrantyReviewStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyReviewStore(inner, WarrantyReviewCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<WarrantyReview> FindWarrantyReviews(IWarrantyReviewStore store, WarrantyReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
