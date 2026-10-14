namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped tax form rules by code.</summary>
public interface ICappedTaxFormRuleStore
{
    CappedTaxFormRule? Find(string code);
    void Save(CappedTaxFormRule item);
    bool Remove(string code);
    List<CappedTaxFormRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedTaxFormRuleStore(Dictionary<string, CappedTaxFormRule> items, List<string> order) : ICappedTaxFormRuleStore
{
    public Dictionary<string, CappedTaxFormRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedTaxFormRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedTaxFormRule item)
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

    public List<CappedTaxFormRule> List()
    {
        var outValue = new List<CappedTaxFormRule>();
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
public sealed class CachedCappedTaxFormRuleStore(MemoryCappedTaxFormRuleStore inner, CappedTaxFormRuleCache cache, int hits, int misses) : ICappedTaxFormRuleStore
{
    public MemoryCappedTaxFormRuleStore Inner { get; set; } = inner;
    public CappedTaxFormRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedTaxFormRule? Find(string code)
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

    public void Save(CappedTaxFormRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedTaxFormRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped tax form rules. Empty fields match all.</summary>
public sealed record CappedTaxFormRuleQuery(string Country, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(CappedTaxFormRule item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<CappedTaxFormRule> Apply(List<CappedTaxFormRule> items)
    {
        var matched = Collections.Filter(items, (CappedTaxFormRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped tax form rule store.</summary>
public static class CappedTaxFormRuleStore
{
    public static ICappedTaxFormRuleStore NewMemoryCappedTaxFormRuleStore()
    {
        return new MemoryCappedTaxFormRuleStore(new Dictionary<string, CappedTaxFormRule>(), new List<string>());
    }

    public static ICappedTaxFormRuleStore NewCachedCappedTaxFormRuleStore(MemoryCappedTaxFormRuleStore inner, int ttl, IClock clock)
    {
        return new CachedCappedTaxFormRuleStore(inner, CappedTaxFormRuleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<CappedTaxFormRule> FindCappedTaxFormRules(ICappedTaxFormRuleStore store, CappedTaxFormRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
