namespace Acme.Workforce.Workplace.FloorPlan.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal floor plan variances by code.</summary>
public interface ISeasonalFloorPlanVarianceStore
{
    SeasonalFloorPlanVariance? Find(string code);
    void Save(SeasonalFloorPlanVariance item);
    bool Remove(string code);
    List<SeasonalFloorPlanVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalFloorPlanVarianceStore(Dictionary<string, SeasonalFloorPlanVariance> items, List<string> order) : ISeasonalFloorPlanVarianceStore
{
    public Dictionary<string, SeasonalFloorPlanVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalFloorPlanVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalFloorPlanVariance item)
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

    public List<SeasonalFloorPlanVariance> List()
    {
        var outValue = new List<SeasonalFloorPlanVariance>();
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
public sealed class CachedSeasonalFloorPlanVarianceStore(MemorySeasonalFloorPlanVarianceStore inner, SeasonalFloorPlanVarianceCache cache, int hits, int misses) : ISeasonalFloorPlanVarianceStore
{
    public MemorySeasonalFloorPlanVarianceStore Inner { get; set; } = inner;
    public SeasonalFloorPlanVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalFloorPlanVariance? Find(string code)
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

    public void Save(SeasonalFloorPlanVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalFloorPlanVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal floor plan variances. Empty fields match all.</summary>
public sealed record SeasonalFloorPlanVarianceQuery(string Department, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalFloorPlanVariance item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<SeasonalFloorPlanVariance> Apply(List<SeasonalFloorPlanVariance> items)
    {
        var matched = Collections.Filter(items, (SeasonalFloorPlanVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal floor plan variance store.</summary>
public static class SeasonalFloorPlanVarianceStore
{
    public static ISeasonalFloorPlanVarianceStore NewMemorySeasonalFloorPlanVarianceStore()
    {
        return new MemorySeasonalFloorPlanVarianceStore(
            new Dictionary<string, SeasonalFloorPlanVariance>(),
            new List<string>());
    }

    public static ISeasonalFloorPlanVarianceStore NewCachedSeasonalFloorPlanVarianceStore(MemorySeasonalFloorPlanVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalFloorPlanVarianceStore(
            inner,
            SeasonalFloorPlanVarianceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalFloorPlanVariance> FindSeasonalFloorPlanVariances(ISeasonalFloorPlanVarianceStore store, SeasonalFloorPlanVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
