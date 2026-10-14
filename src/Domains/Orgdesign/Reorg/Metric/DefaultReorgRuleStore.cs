namespace Acme.Workforce.Orgdesign.Reorg.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default reorg rules by code.</summary>
public interface IDefaultReorgRuleStore
{
    DefaultReorgRule? Find(string code);
    void Save(DefaultReorgRule item);
    bool Remove(string code);
    List<DefaultReorgRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultReorgRuleStore(Dictionary<string, DefaultReorgRule> items, List<string> order) : IDefaultReorgRuleStore
{
    public Dictionary<string, DefaultReorgRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultReorgRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultReorgRule item)
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

    public List<DefaultReorgRule> List()
    {
        var outValue = new List<DefaultReorgRule>();
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
public sealed class CachedDefaultReorgRuleStore(MemoryDefaultReorgRuleStore inner, DefaultReorgRuleCache cache, int hits, int misses) : IDefaultReorgRuleStore
{
    public MemoryDefaultReorgRuleStore Inner { get; set; } = inner;
    public DefaultReorgRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultReorgRule? Find(string code)
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

    public void Save(DefaultReorgRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultReorgRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default reorg rules. Empty fields match all.</summary>
public sealed record DefaultReorgRuleQuery(string Department, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultReorgRule item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultReorgRule> Apply(List<DefaultReorgRule> items)
    {
        var matched = Collections.Filter(items, (DefaultReorgRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default reorg rule store.</summary>
public static class DefaultReorgRuleStore
{
    public static IDefaultReorgRuleStore NewMemoryDefaultReorgRuleStore()
    {
        return new MemoryDefaultReorgRuleStore(new Dictionary<string, DefaultReorgRule>(), new List<string>());
    }

    public static IDefaultReorgRuleStore NewCachedDefaultReorgRuleStore(MemoryDefaultReorgRuleStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultReorgRuleStore(inner, DefaultReorgRuleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<DefaultReorgRule> FindDefaultReorgRules(IDefaultReorgRuleStore store, DefaultReorgRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
