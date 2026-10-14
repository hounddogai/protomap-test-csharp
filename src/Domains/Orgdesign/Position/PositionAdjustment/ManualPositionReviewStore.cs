namespace Acme.Workforce.Orgdesign.Position.PositionAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual position reviews by code.</summary>
public interface IManualPositionReviewStore
{
    ManualPositionReview? Find(string code);
    void Save(ManualPositionReview item);
    bool Remove(string code);
    List<ManualPositionReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualPositionReviewStore(Dictionary<string, ManualPositionReview> items, List<string> order) : IManualPositionReviewStore
{
    public Dictionary<string, ManualPositionReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualPositionReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualPositionReview item)
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

    public List<ManualPositionReview> List()
    {
        var outValue = new List<ManualPositionReview>();
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
public sealed class CachedManualPositionReviewStore(MemoryManualPositionReviewStore inner, ManualPositionReviewCache cache, int hits, int misses) : IManualPositionReviewStore
{
    public MemoryManualPositionReviewStore Inner { get; set; } = inner;
    public ManualPositionReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualPositionReview? Find(string code)
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

    public void Save(ManualPositionReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualPositionReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual position reviews. Empty fields match all.</summary>
public sealed record ManualPositionReviewQuery(string Unit, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(ManualPositionReview item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualPositionReview> Apply(List<ManualPositionReview> items)
    {
        var matched = Collections.Filter(items, (ManualPositionReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual position review store.</summary>
public static class ManualPositionReviewStore
{
    public static IManualPositionReviewStore NewMemoryManualPositionReviewStore()
    {
        return new MemoryManualPositionReviewStore(
            new Dictionary<string, ManualPositionReview>(),
            new List<string>());
    }

    public static IManualPositionReviewStore NewCachedManualPositionReviewStore(MemoryManualPositionReviewStore inner, int ttl, IClock clock)
    {
        return new CachedManualPositionReviewStore(inner, ManualPositionReviewCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ManualPositionReview> FindManualPositionReviews(IManualPositionReviewStore store, ManualPositionReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
