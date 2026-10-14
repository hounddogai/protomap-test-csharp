namespace Acme.Workforce.Orgdesign.CostModel.ModelManifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped cost model rules by code.</summary>
public interface ICappedCostModelRuleStore
{
    CappedCostModelRule? Find(string code);
    void Save(CappedCostModelRule item);
    bool Remove(string code);
    List<CappedCostModelRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedCostModelRuleStore(Dictionary<string, CappedCostModelRule> items, List<string> order) : ICappedCostModelRuleStore
{
    public Dictionary<string, CappedCostModelRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedCostModelRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedCostModelRule item)
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

    public List<CappedCostModelRule> List()
    {
        var outValue = new List<CappedCostModelRule>();
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
public sealed class CachedCappedCostModelRuleStore(MemoryCappedCostModelRuleStore inner, CappedCostModelRuleCache cache, int hits, int misses) : ICappedCostModelRuleStore
{
    public MemoryCappedCostModelRuleStore Inner { get; set; } = inner;
    public CappedCostModelRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedCostModelRule? Find(string code)
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

    public void Save(CappedCostModelRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedCostModelRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped cost model rules. Empty fields match all.</summary>
public sealed record CappedCostModelRuleQuery(string Category, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(CappedCostModelRule item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedCostModelRule> Apply(List<CappedCostModelRule> items)
    {
        var matched = Collections.Filter(items, (CappedCostModelRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped cost model rule store.</summary>
public static class CappedCostModelRuleStore
{
    public static ICappedCostModelRuleStore NewMemoryCappedCostModelRuleStore()
    {
        return new MemoryCappedCostModelRuleStore(new Dictionary<string, CappedCostModelRule>(), new List<string>());
    }

    public static ICappedCostModelRuleStore NewCachedCappedCostModelRuleStore(MemoryCappedCostModelRuleStore inner, int ttl, IClock clock)
    {
        return new CachedCappedCostModelRuleStore(inner, CappedCostModelRuleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<CappedCostModelRule> FindCappedCostModelRules(ICappedCostModelRuleStore store, CappedCostModelRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
