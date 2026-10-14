namespace Acme.Workforce.Compensation.MeritCycle.CycleReview;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy merit cycle reviews by code.</summary>
public interface ILegacyMeritCycleReviewStore
{
    LegacyMeritCycleReview? Find(string code);
    void Save(LegacyMeritCycleReview item);
    bool Remove(string code);
    List<LegacyMeritCycleReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyMeritCycleReviewStore(Dictionary<string, LegacyMeritCycleReview> items, List<string> order) : ILegacyMeritCycleReviewStore
{
    public Dictionary<string, LegacyMeritCycleReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyMeritCycleReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyMeritCycleReview item)
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

    public List<LegacyMeritCycleReview> List()
    {
        var outValue = new List<LegacyMeritCycleReview>();
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
public sealed class CachedLegacyMeritCycleReviewStore(MemoryLegacyMeritCycleReviewStore inner, LegacyMeritCycleReviewCache cache, int hits, int misses) : ILegacyMeritCycleReviewStore
{
    public MemoryLegacyMeritCycleReviewStore Inner { get; set; } = inner;
    public LegacyMeritCycleReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyMeritCycleReview? Find(string code)
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

    public void Save(LegacyMeritCycleReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyMeritCycleReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy merit cycle reviews. Empty fields match all.</summary>
public sealed record LegacyMeritCycleReviewQuery(string Name, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyMeritCycleReview item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<LegacyMeritCycleReview> Apply(List<LegacyMeritCycleReview> items)
    {
        var matched = Collections.Filter(items, (LegacyMeritCycleReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy merit cycle review store.</summary>
public static class LegacyMeritCycleReviewStore
{
    public static ILegacyMeritCycleReviewStore NewMemoryLegacyMeritCycleReviewStore()
    {
        return new MemoryLegacyMeritCycleReviewStore(
            new Dictionary<string, LegacyMeritCycleReview>(),
            new List<string>());
    }

    public static ILegacyMeritCycleReviewStore NewCachedLegacyMeritCycleReviewStore(MemoryLegacyMeritCycleReviewStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyMeritCycleReviewStore(
            inner,
            LegacyMeritCycleReviewCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LegacyMeritCycleReview> FindLegacyMeritCycleReviews(ILegacyMeritCycleReviewStore store, LegacyMeritCycleReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
