namespace Acme.Workforce.Workplace.FloorPlan.PlanEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal floor plan tiers by code.</summary>
public interface ISeasonalFloorPlanTierStore
{
    SeasonalFloorPlanTier? Find(string code);
    void Save(SeasonalFloorPlanTier item);
    bool Remove(string code);
    List<SeasonalFloorPlanTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalFloorPlanTierStore(Dictionary<string, SeasonalFloorPlanTier> items, List<string> order) : ISeasonalFloorPlanTierStore
{
    public Dictionary<string, SeasonalFloorPlanTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalFloorPlanTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalFloorPlanTier item)
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

    public List<SeasonalFloorPlanTier> List()
    {
        var outValue = new List<SeasonalFloorPlanTier>();
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
public sealed class CachedSeasonalFloorPlanTierStore(MemorySeasonalFloorPlanTierStore inner, SeasonalFloorPlanTierCache cache, int hits, int misses) : ISeasonalFloorPlanTierStore
{
    public MemorySeasonalFloorPlanTierStore Inner { get; set; } = inner;
    public SeasonalFloorPlanTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalFloorPlanTier? Find(string code)
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

    public void Save(SeasonalFloorPlanTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalFloorPlanTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal floor plan tiers. Empty fields match all.</summary>
public sealed record SeasonalFloorPlanTierQuery(string Category, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalFloorPlanTier item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<SeasonalFloorPlanTier> Apply(List<SeasonalFloorPlanTier> items)
    {
        var matched = Collections.Filter(items, (SeasonalFloorPlanTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal floor plan tier store.</summary>
public static class SeasonalFloorPlanTierStore
{
    public static ISeasonalFloorPlanTierStore NewMemorySeasonalFloorPlanTierStore()
    {
        return new MemorySeasonalFloorPlanTierStore(
            new Dictionary<string, SeasonalFloorPlanTier>(),
            new List<string>());
    }

    public static ISeasonalFloorPlanTierStore NewCachedSeasonalFloorPlanTierStore(MemorySeasonalFloorPlanTierStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalFloorPlanTierStore(inner, SeasonalFloorPlanTierCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SeasonalFloorPlanTier> FindSeasonalFloorPlanTiers(ISeasonalFloorPlanTierStore store, SeasonalFloorPlanTierQuery query)
    {
        return query.Apply(store.List());
    }
}
