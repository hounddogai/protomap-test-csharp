namespace Acme.Workforce.Compensation.Adjustment.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment rules by code.</summary>
public interface IAdjustmentRuleStore
{
    AdjustmentRule? Find(string code);
    void Save(AdjustmentRule item);
    bool Remove(string code);
    List<AdjustmentRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentRuleStore(Dictionary<string, AdjustmentRule> items, List<string> order) : IAdjustmentRuleStore
{
    public Dictionary<string, AdjustmentRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentRule item)
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

    public List<AdjustmentRule> List()
    {
        var outValue = new List<AdjustmentRule>();
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
public sealed class CachedAdjustmentRuleStore(MemoryAdjustmentRuleStore inner, AdjustmentRuleCache cache, int hits, int misses) : IAdjustmentRuleStore
{
    public MemoryAdjustmentRuleStore Inner { get; set; } = inner;
    public AdjustmentRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentRule? Find(string code)
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

    public void Save(AdjustmentRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment rules. Empty fields match all.</summary>
public sealed record AdjustmentRuleQuery(string Source, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentRule item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<AdjustmentRule> Apply(List<AdjustmentRule> items)
    {
        var matched = Collections.Filter(items, (AdjustmentRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment rule store.</summary>
public static class AdjustmentRuleStore
{
    public static IAdjustmentRuleStore NewMemoryAdjustmentRuleStore()
    {
        return new MemoryAdjustmentRuleStore(new Dictionary<string, AdjustmentRule>(), new List<string>());
    }

    public static IAdjustmentRuleStore NewCachedAdjustmentRuleStore(MemoryAdjustmentRuleStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentRuleStore(inner, AdjustmentRuleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AdjustmentRule> FindAdjustmentRules(IAdjustmentRuleStore store, AdjustmentRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
