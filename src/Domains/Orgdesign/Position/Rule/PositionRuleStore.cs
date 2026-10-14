namespace Acme.Workforce.Orgdesign.Position.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position rules by code.</summary>
public interface IPositionRuleStore
{
    PositionRule? Find(string code);
    void Save(PositionRule item);
    bool Remove(string code);
    List<PositionRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionRuleStore(Dictionary<string, PositionRule> items, List<string> order) : IPositionRuleStore
{
    public Dictionary<string, PositionRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionRule item)
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

    public List<PositionRule> List()
    {
        var outValue = new List<PositionRule>();
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
public sealed class CachedPositionRuleStore(MemoryPositionRuleStore inner, PositionRuleCache cache, int hits, int misses) : IPositionRuleStore
{
    public MemoryPositionRuleStore Inner { get; set; } = inner;
    public PositionRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionRule? Find(string code)
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

    public void Save(PositionRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position rules. Empty fields match all.</summary>
public sealed record PositionRuleQuery(string Label, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PositionRule item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<PositionRule> Apply(List<PositionRule> items)
    {
        var matched = Collections.Filter(items, (PositionRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position rule store.</summary>
public static class PositionRuleStore
{
    public static IPositionRuleStore NewMemoryPositionRuleStore()
    {
        return new MemoryPositionRuleStore(new Dictionary<string, PositionRule>(), new List<string>());
    }

    public static IPositionRuleStore NewCachedPositionRuleStore(MemoryPositionRuleStore inner, int ttl, IClock clock)
    {
        return new CachedPositionRuleStore(inner, PositionRuleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PositionRule> FindPositionRules(IPositionRuleStore store, PositionRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
