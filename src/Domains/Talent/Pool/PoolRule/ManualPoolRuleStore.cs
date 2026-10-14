namespace Acme.Workforce.Talent.Pool.PoolRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual pool rules by code.</summary>
public interface IManualPoolRuleStore
{
    ManualPoolRule? Find(string code);
    void Save(ManualPoolRule item);
    bool Remove(string code);
    List<ManualPoolRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualPoolRuleStore(Dictionary<string, ManualPoolRule> items, List<string> order) : IManualPoolRuleStore
{
    public Dictionary<string, ManualPoolRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualPoolRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualPoolRule item)
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

    public List<ManualPoolRule> List()
    {
        var outValue = new List<ManualPoolRule>();
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
public sealed class CachedManualPoolRuleStore(MemoryManualPoolRuleStore inner, ManualPoolRuleCache cache, int hits, int misses) : IManualPoolRuleStore
{
    public MemoryManualPoolRuleStore Inner { get; set; } = inner;
    public ManualPoolRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualPoolRule? Find(string code)
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

    public void Save(ManualPoolRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualPoolRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual pool rules. Empty fields match all.</summary>
public sealed record ManualPoolRuleQuery(string Name, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(ManualPoolRule item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualPoolRule> Apply(List<ManualPoolRule> items)
    {
        var matched = Collections.Filter(items, (ManualPoolRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual pool rule store.</summary>
public static class ManualPoolRuleStore
{
    public static IManualPoolRuleStore NewMemoryManualPoolRuleStore()
    {
        return new MemoryManualPoolRuleStore(new Dictionary<string, ManualPoolRule>(), new List<string>());
    }

    public static IManualPoolRuleStore NewCachedManualPoolRuleStore(MemoryManualPoolRuleStore inner, int ttl, IClock clock)
    {
        return new CachedManualPoolRuleStore(inner, ManualPoolRuleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ManualPoolRule> FindManualPoolRules(IManualPoolRuleStore store, ManualPoolRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
