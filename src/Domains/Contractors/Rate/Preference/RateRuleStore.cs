namespace Acme.Workforce.Contractors.Rate.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate rules by code.</summary>
public interface IRateRuleStore
{
    RateRule? Find(string code);
    void Save(RateRule item);
    bool Remove(string code);
    List<RateRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateRuleStore(Dictionary<string, RateRule> items, List<string> order) : IRateRuleStore
{
    public Dictionary<string, RateRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateRule item)
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

    public List<RateRule> List()
    {
        var outValue = new List<RateRule>();
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
public sealed class CachedRateRuleStore(MemoryRateRuleStore inner, RateRuleCache cache, int hits, int misses) : IRateRuleStore
{
    public MemoryRateRuleStore Inner { get; set; } = inner;
    public RateRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateRule? Find(string code)
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

    public void Save(RateRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate rules. Empty fields match all.</summary>
public sealed record RateRuleQuery(string Country, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(RateRule item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RateRule> Apply(List<RateRule> items)
    {
        var matched = Collections.Filter(items, (RateRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate rule store.</summary>
public static class RateRuleStore
{
    public static IRateRuleStore NewMemoryRateRuleStore()
    {
        return new MemoryRateRuleStore(new Dictionary<string, RateRule>(), new List<string>());
    }

    public static IRateRuleStore NewCachedRateRuleStore(MemoryRateRuleStore inner, int ttl, IClock clock)
    {
        return new CachedRateRuleStore(inner, RateRuleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RateRule> FindRateRules(IRateRuleStore store, RateRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
