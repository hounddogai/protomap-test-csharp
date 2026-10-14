namespace Acme.Workforce.Workplace.FloorPlan.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy floor plan rules by code.</summary>
public interface ILegacyFloorPlanRuleStore
{
    LegacyFloorPlanRule? Find(string code);
    void Save(LegacyFloorPlanRule item);
    bool Remove(string code);
    List<LegacyFloorPlanRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyFloorPlanRuleStore(Dictionary<string, LegacyFloorPlanRule> items, List<string> order) : ILegacyFloorPlanRuleStore
{
    public Dictionary<string, LegacyFloorPlanRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyFloorPlanRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyFloorPlanRule item)
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

    public List<LegacyFloorPlanRule> List()
    {
        var outValue = new List<LegacyFloorPlanRule>();
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
public sealed class CachedLegacyFloorPlanRuleStore(MemoryLegacyFloorPlanRuleStore inner, LegacyFloorPlanRuleCache cache, int hits, int misses) : ILegacyFloorPlanRuleStore
{
    public MemoryLegacyFloorPlanRuleStore Inner { get; set; } = inner;
    public LegacyFloorPlanRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyFloorPlanRule? Find(string code)
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

    public void Save(LegacyFloorPlanRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyFloorPlanRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy floor plan rules. Empty fields match all.</summary>
public sealed record LegacyFloorPlanRuleQuery(string Label, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyFloorPlanRule item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyFloorPlanRule> Apply(List<LegacyFloorPlanRule> items)
    {
        var matched = Collections.Filter(items, (LegacyFloorPlanRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy floor plan rule store.</summary>
public static class LegacyFloorPlanRuleStore
{
    public static ILegacyFloorPlanRuleStore NewMemoryLegacyFloorPlanRuleStore()
    {
        return new MemoryLegacyFloorPlanRuleStore(new Dictionary<string, LegacyFloorPlanRule>(), new List<string>());
    }

    public static ILegacyFloorPlanRuleStore NewCachedLegacyFloorPlanRuleStore(MemoryLegacyFloorPlanRuleStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyFloorPlanRuleStore(inner, LegacyFloorPlanRuleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LegacyFloorPlanRule> FindLegacyFloorPlanRules(ILegacyFloorPlanRuleStore store, LegacyFloorPlanRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
