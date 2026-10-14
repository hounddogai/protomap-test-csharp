namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan tiers by code.</summary>
public interface ISuccessionPlanTierStore
{
    SuccessionPlanTier? Find(string code);
    void Save(SuccessionPlanTier item);
    bool Remove(string code);
    List<SuccessionPlanTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanTierStore(Dictionary<string, SuccessionPlanTier> items, List<string> order) : ISuccessionPlanTierStore
{
    public Dictionary<string, SuccessionPlanTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanTier item)
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

    public List<SuccessionPlanTier> List()
    {
        var outValue = new List<SuccessionPlanTier>();
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
public sealed class CachedSuccessionPlanTierStore(MemorySuccessionPlanTierStore inner, SuccessionPlanTierCache cache, int hits, int misses) : ISuccessionPlanTierStore
{
    public MemorySuccessionPlanTierStore Inner { get; set; } = inner;
    public SuccessionPlanTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanTier? Find(string code)
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

    public void Save(SuccessionPlanTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan tiers. Empty fields match all.</summary>
public sealed record SuccessionPlanTierQuery(string Title, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanTier item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SuccessionPlanTier> Apply(List<SuccessionPlanTier> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan tier store.</summary>
public static class SuccessionPlanTierStore
{
    public static ISuccessionPlanTierStore NewMemorySuccessionPlanTierStore()
    {
        return new MemorySuccessionPlanTierStore(new Dictionary<string, SuccessionPlanTier>(), new List<string>());
    }

    public static ISuccessionPlanTierStore NewCachedSuccessionPlanTierStore(MemorySuccessionPlanTierStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanTierStore(inner, SuccessionPlanTierCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SuccessionPlanTier> FindSuccessionPlanTiers(ISuccessionPlanTierStore store, SuccessionPlanTierQuery query)
    {
        return query.Apply(store.List());
    }
}
