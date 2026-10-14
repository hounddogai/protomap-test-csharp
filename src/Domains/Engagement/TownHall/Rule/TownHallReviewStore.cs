namespace Acme.Workforce.Engagement.TownHall.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall reviews by code.</summary>
public interface ITownHallReviewStore
{
    TownHallReview? Find(string code);
    void Save(TownHallReview item);
    bool Remove(string code);
    List<TownHallReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallReviewStore(Dictionary<string, TownHallReview> items, List<string> order) : ITownHallReviewStore
{
    public Dictionary<string, TownHallReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallReview item)
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

    public List<TownHallReview> List()
    {
        var outValue = new List<TownHallReview>();
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
public sealed class CachedTownHallReviewStore(MemoryTownHallReviewStore inner, TownHallReviewCache cache, int hits, int misses) : ITownHallReviewStore
{
    public MemoryTownHallReviewStore Inner { get; set; } = inner;
    public TownHallReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallReview? Find(string code)
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

    public void Save(TownHallReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall reviews. Empty fields match all.</summary>
public sealed record TownHallReviewQuery(string Channel, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallReview item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TownHallReview> Apply(List<TownHallReview> items)
    {
        var matched = Collections.Filter(items, (TownHallReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall review store.</summary>
public static class TownHallReviewStore
{
    public static ITownHallReviewStore NewMemoryTownHallReviewStore()
    {
        return new MemoryTownHallReviewStore(new Dictionary<string, TownHallReview>(), new List<string>());
    }

    public static ITownHallReviewStore NewCachedTownHallReviewStore(MemoryTownHallReviewStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallReviewStore(inner, TownHallReviewCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TownHallReview> FindTownHallReviews(ITownHallReviewStore store, TownHallReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
