namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form rules by code.</summary>
public interface ITaxFormRuleStore
{
    TaxFormRule? Find(string code);
    void Save(TaxFormRule item);
    bool Remove(string code);
    List<TaxFormRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormRuleStore(Dictionary<string, TaxFormRule> items, List<string> order) : ITaxFormRuleStore
{
    public Dictionary<string, TaxFormRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormRule item)
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

    public List<TaxFormRule> List()
    {
        var outValue = new List<TaxFormRule>();
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
public sealed class CachedTaxFormRuleStore(MemoryTaxFormRuleStore inner, TaxFormRuleCache cache, int hits, int misses) : ITaxFormRuleStore
{
    public MemoryTaxFormRuleStore Inner { get; set; } = inner;
    public TaxFormRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormRule? Find(string code)
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

    public void Save(TaxFormRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form rules. Empty fields match all.</summary>
public sealed record TaxFormRuleQuery(string Region, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormRule item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<TaxFormRule> Apply(List<TaxFormRule> items)
    {
        var matched = Collections.Filter(items, (TaxFormRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form rule store.</summary>
public static class TaxFormRuleStore
{
    public static ITaxFormRuleStore NewMemoryTaxFormRuleStore()
    {
        return new MemoryTaxFormRuleStore(new Dictionary<string, TaxFormRule>(), new List<string>());
    }

    public static ITaxFormRuleStore NewCachedTaxFormRuleStore(MemoryTaxFormRuleStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormRuleStore(inner, TaxFormRuleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TaxFormRule> FindTaxFormRules(ITaxFormRuleStore store, TaxFormRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
