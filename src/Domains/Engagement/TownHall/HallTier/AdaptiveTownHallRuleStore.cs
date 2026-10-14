namespace Acme.Workforce.Engagement.TownHall.HallTier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive town hall rules by code.</summary>
public interface IAdaptiveTownHallRuleStore
{
    AdaptiveTownHallRule? Find(string code);
    void Save(AdaptiveTownHallRule item);
    bool Remove(string code);
    List<AdaptiveTownHallRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveTownHallRuleStore(Dictionary<string, AdaptiveTownHallRule> items, List<string> order) : IAdaptiveTownHallRuleStore
{
    public Dictionary<string, AdaptiveTownHallRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveTownHallRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveTownHallRule item)
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

    public List<AdaptiveTownHallRule> List()
    {
        var outValue = new List<AdaptiveTownHallRule>();
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
public sealed class CachedAdaptiveTownHallRuleStore(MemoryAdaptiveTownHallRuleStore inner, AdaptiveTownHallRuleCache cache, int hits, int misses) : IAdaptiveTownHallRuleStore
{
    public MemoryAdaptiveTownHallRuleStore Inner { get; set; } = inner;
    public AdaptiveTownHallRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveTownHallRule? Find(string code)
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

    public void Save(AdaptiveTownHallRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveTownHallRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive town hall rules. Empty fields match all.</summary>
public sealed record AdaptiveTownHallRuleQuery(string Department, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveTownHallRule item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<AdaptiveTownHallRule> Apply(List<AdaptiveTownHallRule> items)
    {
        var matched = Collections.Filter(items, (AdaptiveTownHallRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive town hall rule store.</summary>
public static class AdaptiveTownHallRuleStore
{
    public static IAdaptiveTownHallRuleStore NewMemoryAdaptiveTownHallRuleStore()
    {
        return new MemoryAdaptiveTownHallRuleStore(
            new Dictionary<string, AdaptiveTownHallRule>(),
            new List<string>());
    }

    public static IAdaptiveTownHallRuleStore NewCachedAdaptiveTownHallRuleStore(MemoryAdaptiveTownHallRuleStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveTownHallRuleStore(inner, AdaptiveTownHallRuleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AdaptiveTownHallRule> FindAdaptiveTownHallRules(IAdaptiveTownHallRuleStore store, AdaptiveTownHallRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
