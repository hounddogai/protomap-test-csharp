namespace Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual adjustment rules by code.</summary>
public interface IManualAdjustmentRuleStore
{
    ManualAdjustmentRule? Find(string code);
    void Save(ManualAdjustmentRule item);
    bool Remove(string code);
    List<ManualAdjustmentRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualAdjustmentRuleStore(Dictionary<string, ManualAdjustmentRule> items, List<string> order) : IManualAdjustmentRuleStore
{
    public Dictionary<string, ManualAdjustmentRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualAdjustmentRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualAdjustmentRule item)
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

    public List<ManualAdjustmentRule> List()
    {
        var outValue = new List<ManualAdjustmentRule>();
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
public sealed class CachedManualAdjustmentRuleStore(MemoryManualAdjustmentRuleStore inner, ManualAdjustmentRuleCache cache, int hits, int misses) : IManualAdjustmentRuleStore
{
    public MemoryManualAdjustmentRuleStore Inner { get; set; } = inner;
    public ManualAdjustmentRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualAdjustmentRule? Find(string code)
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

    public void Save(ManualAdjustmentRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualAdjustmentRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual adjustment rules. Empty fields match all.</summary>
public sealed record ManualAdjustmentRuleQuery(string Department, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(ManualAdjustmentRule item)
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

    public List<ManualAdjustmentRule> Apply(List<ManualAdjustmentRule> items)
    {
        var matched = Collections.Filter(items, (ManualAdjustmentRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual adjustment rule store.</summary>
public static class ManualAdjustmentRuleStore
{
    public static IManualAdjustmentRuleStore NewMemoryManualAdjustmentRuleStore()
    {
        return new MemoryManualAdjustmentRuleStore(
            new Dictionary<string, ManualAdjustmentRule>(),
            new List<string>());
    }

    public static IManualAdjustmentRuleStore NewCachedManualAdjustmentRuleStore(MemoryManualAdjustmentRuleStore inner, int ttl, IClock clock)
    {
        return new CachedManualAdjustmentRuleStore(inner, ManualAdjustmentRuleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ManualAdjustmentRule> FindManualAdjustmentRules(IManualAdjustmentRuleStore store, ManualAdjustmentRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
