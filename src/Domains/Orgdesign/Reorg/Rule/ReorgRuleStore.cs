namespace Acme.Workforce.Orgdesign.Reorg.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg rules by code.</summary>
public interface IReorgRuleStore
{
    ReorgRule? Find(string code);
    void Save(ReorgRule item);
    bool Remove(string code);
    List<ReorgRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgRuleStore(Dictionary<string, ReorgRule> items, List<string> order) : IReorgRuleStore
{
    public Dictionary<string, ReorgRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgRule item)
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

    public List<ReorgRule> List()
    {
        var outValue = new List<ReorgRule>();
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
public sealed class CachedReorgRuleStore(MemoryReorgRuleStore inner, ReorgRuleCache cache, int hits, int misses) : IReorgRuleStore
{
    public MemoryReorgRuleStore Inner { get; set; } = inner;
    public ReorgRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgRule? Find(string code)
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

    public void Save(ReorgRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg rules. Empty fields match all.</summary>
public sealed record ReorgRuleQuery(string Name, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgRule item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReorgRule> Apply(List<ReorgRule> items)
    {
        var matched = Collections.Filter(items, (ReorgRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg rule store.</summary>
public static class ReorgRuleStore
{
    public static IReorgRuleStore NewMemoryReorgRuleStore()
    {
        return new MemoryReorgRuleStore(new Dictionary<string, ReorgRule>(), new List<string>());
    }

    public static IReorgRuleStore NewCachedReorgRuleStore(MemoryReorgRuleStore inner, int ttl, IClock clock)
    {
        return new CachedReorgRuleStore(inner, ReorgRuleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ReorgRule> FindReorgRules(IReorgRuleStore store, ReorgRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
