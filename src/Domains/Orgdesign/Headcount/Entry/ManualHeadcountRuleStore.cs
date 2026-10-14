namespace Acme.Workforce.Orgdesign.Headcount.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual headcount rules by code.</summary>
public interface IManualHeadcountRuleStore
{
    ManualHeadcountRule? Find(string code);
    void Save(ManualHeadcountRule item);
    bool Remove(string code);
    List<ManualHeadcountRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualHeadcountRuleStore(Dictionary<string, ManualHeadcountRule> items, List<string> order) : IManualHeadcountRuleStore
{
    public Dictionary<string, ManualHeadcountRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualHeadcountRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualHeadcountRule item)
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

    public List<ManualHeadcountRule> List()
    {
        var outValue = new List<ManualHeadcountRule>();
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
public sealed class CachedManualHeadcountRuleStore(MemoryManualHeadcountRuleStore inner, ManualHeadcountRuleCache cache, int hits, int misses) : IManualHeadcountRuleStore
{
    public MemoryManualHeadcountRuleStore Inner { get; set; } = inner;
    public ManualHeadcountRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualHeadcountRule? Find(string code)
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

    public void Save(ManualHeadcountRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualHeadcountRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual headcount rules. Empty fields match all.</summary>
public sealed record ManualHeadcountRuleQuery(string Name, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(ManualHeadcountRule item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualHeadcountRule> Apply(List<ManualHeadcountRule> items)
    {
        var matched = Collections.Filter(items, (ManualHeadcountRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual headcount rule store.</summary>
public static class ManualHeadcountRuleStore
{
    public static IManualHeadcountRuleStore NewMemoryManualHeadcountRuleStore()
    {
        return new MemoryManualHeadcountRuleStore(new Dictionary<string, ManualHeadcountRule>(), new List<string>());
    }

    public static IManualHeadcountRuleStore NewCachedManualHeadcountRuleStore(MemoryManualHeadcountRuleStore inner, int ttl, IClock clock)
    {
        return new CachedManualHeadcountRuleStore(inner, ManualHeadcountRuleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ManualHeadcountRule> FindManualHeadcountRules(IManualHeadcountRuleStore store, ManualHeadcountRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
