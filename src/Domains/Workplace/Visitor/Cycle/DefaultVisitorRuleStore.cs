namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default visitor rules by code.</summary>
public interface IDefaultVisitorRuleStore
{
    DefaultVisitorRule? Find(string code);
    void Save(DefaultVisitorRule item);
    bool Remove(string code);
    List<DefaultVisitorRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultVisitorRuleStore(Dictionary<string, DefaultVisitorRule> items, List<string> order) : IDefaultVisitorRuleStore
{
    public Dictionary<string, DefaultVisitorRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultVisitorRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultVisitorRule item)
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

    public List<DefaultVisitorRule> List()
    {
        var outValue = new List<DefaultVisitorRule>();
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
public sealed class CachedDefaultVisitorRuleStore(MemoryDefaultVisitorRuleStore inner, DefaultVisitorRuleCache cache, int hits, int misses) : IDefaultVisitorRuleStore
{
    public MemoryDefaultVisitorRuleStore Inner { get; set; } = inner;
    public DefaultVisitorRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultVisitorRule? Find(string code)
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

    public void Save(DefaultVisitorRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultVisitorRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default visitor rules. Empty fields match all.</summary>
public sealed record DefaultVisitorRuleQuery(string Segment, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultVisitorRule item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<DefaultVisitorRule> Apply(List<DefaultVisitorRule> items)
    {
        var matched = Collections.Filter(items, (DefaultVisitorRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default visitor rule store.</summary>
public static class DefaultVisitorRuleStore
{
    public static IDefaultVisitorRuleStore NewMemoryDefaultVisitorRuleStore()
    {
        return new MemoryDefaultVisitorRuleStore(new Dictionary<string, DefaultVisitorRule>(), new List<string>());
    }

    public static IDefaultVisitorRuleStore NewCachedDefaultVisitorRuleStore(MemoryDefaultVisitorRuleStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultVisitorRuleStore(inner, DefaultVisitorRuleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<DefaultVisitorRule> FindDefaultVisitorRules(IDefaultVisitorRuleStore store, DefaultVisitorRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
