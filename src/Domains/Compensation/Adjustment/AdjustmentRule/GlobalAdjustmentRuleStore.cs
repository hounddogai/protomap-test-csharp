namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global adjustment rules by code.</summary>
public interface IGlobalAdjustmentRuleStore
{
    GlobalAdjustmentRule? Find(string code);
    void Save(GlobalAdjustmentRule item);
    bool Remove(string code);
    List<GlobalAdjustmentRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalAdjustmentRuleStore(Dictionary<string, GlobalAdjustmentRule> items, List<string> order) : IGlobalAdjustmentRuleStore
{
    public Dictionary<string, GlobalAdjustmentRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalAdjustmentRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalAdjustmentRule item)
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

    public List<GlobalAdjustmentRule> List()
    {
        var outValue = new List<GlobalAdjustmentRule>();
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
public sealed class CachedGlobalAdjustmentRuleStore(MemoryGlobalAdjustmentRuleStore inner, GlobalAdjustmentRuleCache cache, int hits, int misses) : IGlobalAdjustmentRuleStore
{
    public MemoryGlobalAdjustmentRuleStore Inner { get; set; } = inner;
    public GlobalAdjustmentRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalAdjustmentRule? Find(string code)
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

    public void Save(GlobalAdjustmentRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalAdjustmentRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global adjustment rules. Empty fields match all.</summary>
public sealed record GlobalAdjustmentRuleQuery(string Label, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalAdjustmentRule item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalAdjustmentRule> Apply(List<GlobalAdjustmentRule> items)
    {
        var matched = Collections.Filter(items, (GlobalAdjustmentRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global adjustment rule store.</summary>
public static class GlobalAdjustmentRuleStore
{
    public static IGlobalAdjustmentRuleStore NewMemoryGlobalAdjustmentRuleStore()
    {
        return new MemoryGlobalAdjustmentRuleStore(
            new Dictionary<string, GlobalAdjustmentRule>(),
            new List<string>());
    }

    public static IGlobalAdjustmentRuleStore NewCachedGlobalAdjustmentRuleStore(MemoryGlobalAdjustmentRuleStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalAdjustmentRuleStore(inner, GlobalAdjustmentRuleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<GlobalAdjustmentRule> FindGlobalAdjustmentRules(IGlobalAdjustmentRuleStore store, GlobalAdjustmentRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
