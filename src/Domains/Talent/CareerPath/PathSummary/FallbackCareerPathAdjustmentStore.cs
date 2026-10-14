namespace Acme.Workforce.Talent.CareerPath.PathSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback career path adjustments by code.</summary>
public interface IFallbackCareerPathAdjustmentStore
{
    FallbackCareerPathAdjustment? Find(string code);
    void Save(FallbackCareerPathAdjustment item);
    bool Remove(string code);
    List<FallbackCareerPathAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackCareerPathAdjustmentStore(Dictionary<string, FallbackCareerPathAdjustment> items, List<string> order) : IFallbackCareerPathAdjustmentStore
{
    public Dictionary<string, FallbackCareerPathAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackCareerPathAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackCareerPathAdjustment item)
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

    public List<FallbackCareerPathAdjustment> List()
    {
        var outValue = new List<FallbackCareerPathAdjustment>();
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
public sealed class CachedFallbackCareerPathAdjustmentStore(MemoryFallbackCareerPathAdjustmentStore inner, FallbackCareerPathAdjustmentCache cache, int hits, int misses) : IFallbackCareerPathAdjustmentStore
{
    public MemoryFallbackCareerPathAdjustmentStore Inner { get; set; } = inner;
    public FallbackCareerPathAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackCareerPathAdjustment? Find(string code)
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

    public void Save(FallbackCareerPathAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackCareerPathAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback career path adjustments. Empty fields match all.</summary>
public sealed record FallbackCareerPathAdjustmentQuery(string Label, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackCareerPathAdjustment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<FallbackCareerPathAdjustment> Apply(List<FallbackCareerPathAdjustment> items)
    {
        var matched = Collections.Filter(items, (FallbackCareerPathAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback career path adjustment store.</summary>
public static class FallbackCareerPathAdjustmentStore
{
    public static IFallbackCareerPathAdjustmentStore NewMemoryFallbackCareerPathAdjustmentStore()
    {
        return new MemoryFallbackCareerPathAdjustmentStore(
            new Dictionary<string, FallbackCareerPathAdjustment>(),
            new List<string>());
    }

    public static IFallbackCareerPathAdjustmentStore NewCachedFallbackCareerPathAdjustmentStore(MemoryFallbackCareerPathAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackCareerPathAdjustmentStore(
            inner,
            FallbackCareerPathAdjustmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<FallbackCareerPathAdjustment> FindFallbackCareerPathAdjustments(IFallbackCareerPathAdjustmentStore store, FallbackCareerPathAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
