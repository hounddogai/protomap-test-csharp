namespace Acme.Workforce.Assets.Laptop.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop rules by code.</summary>
public interface ILaptopRuleStore
{
    LaptopRule? Find(string code);
    void Save(LaptopRule item);
    bool Remove(string code);
    List<LaptopRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopRuleStore(Dictionary<string, LaptopRule> items, List<string> order) : ILaptopRuleStore
{
    public Dictionary<string, LaptopRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopRule item)
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

    public List<LaptopRule> List()
    {
        var outValue = new List<LaptopRule>();
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
public sealed class CachedLaptopRuleStore(MemoryLaptopRuleStore inner, LaptopRuleCache cache, int hits, int misses) : ILaptopRuleStore
{
    public MemoryLaptopRuleStore Inner { get; set; } = inner;
    public LaptopRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopRule? Find(string code)
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

    public void Save(LaptopRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop rules. Empty fields match all.</summary>
public sealed record LaptopRuleQuery(string Unit, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopRule item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<LaptopRule> Apply(List<LaptopRule> items)
    {
        var matched = Collections.Filter(items, (LaptopRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop rule store.</summary>
public static class LaptopRuleStore
{
    public static ILaptopRuleStore NewMemoryLaptopRuleStore()
    {
        return new MemoryLaptopRuleStore(new Dictionary<string, LaptopRule>(), new List<string>());
    }

    public static ILaptopRuleStore NewCachedLaptopRuleStore(MemoryLaptopRuleStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopRuleStore(inner, LaptopRuleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LaptopRule> FindLaptopRules(ILaptopRuleStore store, LaptopRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
