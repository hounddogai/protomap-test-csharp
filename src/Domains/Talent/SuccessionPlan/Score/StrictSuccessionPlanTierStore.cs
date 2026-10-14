namespace Acme.Workforce.Talent.SuccessionPlan.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict succession plan tiers by code.</summary>
public interface IStrictSuccessionPlanTierStore
{
    StrictSuccessionPlanTier? Find(string code);
    void Save(StrictSuccessionPlanTier item);
    bool Remove(string code);
    List<StrictSuccessionPlanTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictSuccessionPlanTierStore(Dictionary<string, StrictSuccessionPlanTier> items, List<string> order) : IStrictSuccessionPlanTierStore
{
    public Dictionary<string, StrictSuccessionPlanTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictSuccessionPlanTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictSuccessionPlanTier item)
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

    public List<StrictSuccessionPlanTier> List()
    {
        var outValue = new List<StrictSuccessionPlanTier>();
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
public sealed class CachedStrictSuccessionPlanTierStore(MemoryStrictSuccessionPlanTierStore inner, StrictSuccessionPlanTierCache cache, int hits, int misses) : IStrictSuccessionPlanTierStore
{
    public MemoryStrictSuccessionPlanTierStore Inner { get; set; } = inner;
    public StrictSuccessionPlanTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictSuccessionPlanTier? Find(string code)
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

    public void Save(StrictSuccessionPlanTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictSuccessionPlanTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict succession plan tiers. Empty fields match all.</summary>
public sealed record StrictSuccessionPlanTierQuery(string Name, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(StrictSuccessionPlanTier item)
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

    public List<StrictSuccessionPlanTier> Apply(List<StrictSuccessionPlanTier> items)
    {
        var matched = Collections.Filter(items, (StrictSuccessionPlanTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict succession plan tier store.</summary>
public static class StrictSuccessionPlanTierStore
{
    public static IStrictSuccessionPlanTierStore NewMemoryStrictSuccessionPlanTierStore()
    {
        return new MemoryStrictSuccessionPlanTierStore(
            new Dictionary<string, StrictSuccessionPlanTier>(),
            new List<string>());
    }

    public static IStrictSuccessionPlanTierStore NewCachedStrictSuccessionPlanTierStore(MemoryStrictSuccessionPlanTierStore inner, int ttl, IClock clock)
    {
        return new CachedStrictSuccessionPlanTierStore(
            inner,
            StrictSuccessionPlanTierCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<StrictSuccessionPlanTier> FindStrictSuccessionPlanTiers(IStrictSuccessionPlanTierStore store, StrictSuccessionPlanTierQuery query)
    {
        return query.Apply(store.List());
    }
}
