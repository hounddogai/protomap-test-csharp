namespace Acme.Workforce.Assets.Assignment.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback assignment adjustments by code.</summary>
public interface IFallbackAssignmentAdjustmentStore
{
    FallbackAssignmentAdjustment? Find(string code);
    void Save(FallbackAssignmentAdjustment item);
    bool Remove(string code);
    List<FallbackAssignmentAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackAssignmentAdjustmentStore(Dictionary<string, FallbackAssignmentAdjustment> items, List<string> order) : IFallbackAssignmentAdjustmentStore
{
    public Dictionary<string, FallbackAssignmentAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackAssignmentAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackAssignmentAdjustment item)
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

    public List<FallbackAssignmentAdjustment> List()
    {
        var outValue = new List<FallbackAssignmentAdjustment>();
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
public sealed class CachedFallbackAssignmentAdjustmentStore(MemoryFallbackAssignmentAdjustmentStore inner, FallbackAssignmentAdjustmentCache cache, int hits, int misses) : IFallbackAssignmentAdjustmentStore
{
    public MemoryFallbackAssignmentAdjustmentStore Inner { get; set; } = inner;
    public FallbackAssignmentAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackAssignmentAdjustment? Find(string code)
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

    public void Save(FallbackAssignmentAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackAssignmentAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback assignment adjustments. Empty fields match all.</summary>
public sealed record FallbackAssignmentAdjustmentQuery(string Title, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackAssignmentAdjustment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackAssignmentAdjustment> Apply(List<FallbackAssignmentAdjustment> items)
    {
        var matched = Collections.Filter(items, (FallbackAssignmentAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback assignment adjustment store.</summary>
public static class FallbackAssignmentAdjustmentStore
{
    public static IFallbackAssignmentAdjustmentStore NewMemoryFallbackAssignmentAdjustmentStore()
    {
        return new MemoryFallbackAssignmentAdjustmentStore(
            new Dictionary<string, FallbackAssignmentAdjustment>(),
            new List<string>());
    }

    public static IFallbackAssignmentAdjustmentStore NewCachedFallbackAssignmentAdjustmentStore(MemoryFallbackAssignmentAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackAssignmentAdjustmentStore(
            inner,
            FallbackAssignmentAdjustmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<FallbackAssignmentAdjustment> FindFallbackAssignmentAdjustments(IFallbackAssignmentAdjustmentStore store, FallbackAssignmentAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
