namespace Acme.Workforce.Workplace.Desk.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk reviews by code.</summary>
public interface IDeskReviewStore
{
    DeskReview? Find(string code);
    void Save(DeskReview item);
    bool Remove(string code);
    List<DeskReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskReviewStore(Dictionary<string, DeskReview> items, List<string> order) : IDeskReviewStore
{
    public Dictionary<string, DeskReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskReview item)
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

    public List<DeskReview> List()
    {
        var outValue = new List<DeskReview>();
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
public sealed class CachedDeskReviewStore(MemoryDeskReviewStore inner, DeskReviewCache cache, int hits, int misses) : IDeskReviewStore
{
    public MemoryDeskReviewStore Inner { get; set; } = inner;
    public DeskReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskReview? Find(string code)
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

    public void Save(DeskReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk reviews. Empty fields match all.</summary>
public sealed record DeskReviewQuery(string Name, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(DeskReview item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<DeskReview> Apply(List<DeskReview> items)
    {
        var matched = Collections.Filter(items, (DeskReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk review store.</summary>
public static class DeskReviewStore
{
    public static IDeskReviewStore NewMemoryDeskReviewStore()
    {
        return new MemoryDeskReviewStore(new Dictionary<string, DeskReview>(), new List<string>());
    }

    public static IDeskReviewStore NewCachedDeskReviewStore(MemoryDeskReviewStore inner, int ttl, IClock clock)
    {
        return new CachedDeskReviewStore(inner, DeskReviewCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DeskReview> FindDeskReviews(IDeskReviewStore store, DeskReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
