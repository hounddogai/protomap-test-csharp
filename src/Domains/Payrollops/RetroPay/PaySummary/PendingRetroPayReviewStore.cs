namespace Acme.Workforce.Payrollops.RetroPay.PaySummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending retro pay reviews by code.</summary>
public interface IPendingRetroPayReviewStore
{
    PendingRetroPayReview? Find(string code);
    void Save(PendingRetroPayReview item);
    bool Remove(string code);
    List<PendingRetroPayReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingRetroPayReviewStore(Dictionary<string, PendingRetroPayReview> items, List<string> order) : IPendingRetroPayReviewStore
{
    public Dictionary<string, PendingRetroPayReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingRetroPayReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingRetroPayReview item)
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

    public List<PendingRetroPayReview> List()
    {
        var outValue = new List<PendingRetroPayReview>();
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
public sealed class CachedPendingRetroPayReviewStore(MemoryPendingRetroPayReviewStore inner, PendingRetroPayReviewCache cache, int hits, int misses) : IPendingRetroPayReviewStore
{
    public MemoryPendingRetroPayReviewStore Inner { get; set; } = inner;
    public PendingRetroPayReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingRetroPayReview? Find(string code)
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

    public void Save(PendingRetroPayReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingRetroPayReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending retro pay reviews. Empty fields match all.</summary>
public sealed record PendingRetroPayReviewQuery(string Name, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(PendingRetroPayReview item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingRetroPayReview> Apply(List<PendingRetroPayReview> items)
    {
        var matched = Collections.Filter(items, (PendingRetroPayReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending retro pay review store.</summary>
public static class PendingRetroPayReviewStore
{
    public static IPendingRetroPayReviewStore NewMemoryPendingRetroPayReviewStore()
    {
        return new MemoryPendingRetroPayReviewStore(
            new Dictionary<string, PendingRetroPayReview>(),
            new List<string>());
    }

    public static IPendingRetroPayReviewStore NewCachedPendingRetroPayReviewStore(MemoryPendingRetroPayReviewStore inner, int ttl, IClock clock)
    {
        return new CachedPendingRetroPayReviewStore(inner, PendingRetroPayReviewCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PendingRetroPayReview> FindPendingRetroPayReviews(IPendingRetroPayReviewStore store, PendingRetroPayReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
