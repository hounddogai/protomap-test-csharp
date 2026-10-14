namespace Acme.Workforce.Payrollops.Deduction.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict deduction rules by code.</summary>
public interface IStrictDeductionRuleStore
{
    StrictDeductionRule? Find(string code);
    void Save(StrictDeductionRule item);
    bool Remove(string code);
    List<StrictDeductionRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictDeductionRuleStore(Dictionary<string, StrictDeductionRule> items, List<string> order) : IStrictDeductionRuleStore
{
    public Dictionary<string, StrictDeductionRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictDeductionRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictDeductionRule item)
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

    public List<StrictDeductionRule> List()
    {
        var outValue = new List<StrictDeductionRule>();
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
public sealed class CachedStrictDeductionRuleStore(MemoryStrictDeductionRuleStore inner, StrictDeductionRuleCache cache, int hits, int misses) : IStrictDeductionRuleStore
{
    public MemoryStrictDeductionRuleStore Inner { get; set; } = inner;
    public StrictDeductionRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictDeductionRule? Find(string code)
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

    public void Save(StrictDeductionRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictDeductionRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict deduction rules. Empty fields match all.</summary>
public sealed record StrictDeductionRuleQuery(string Label, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(StrictDeductionRule item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictDeductionRule> Apply(List<StrictDeductionRule> items)
    {
        var matched = Collections.Filter(items, (StrictDeductionRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict deduction rule store.</summary>
public static class StrictDeductionRuleStore
{
    public static IStrictDeductionRuleStore NewMemoryStrictDeductionRuleStore()
    {
        return new MemoryStrictDeductionRuleStore(new Dictionary<string, StrictDeductionRule>(), new List<string>());
    }

    public static IStrictDeductionRuleStore NewCachedStrictDeductionRuleStore(MemoryStrictDeductionRuleStore inner, int ttl, IClock clock)
    {
        return new CachedStrictDeductionRuleStore(inner, StrictDeductionRuleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<StrictDeductionRule> FindStrictDeductionRules(IStrictDeductionRuleStore store, StrictDeductionRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
