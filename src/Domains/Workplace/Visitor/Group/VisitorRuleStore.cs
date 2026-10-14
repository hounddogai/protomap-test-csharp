namespace Acme.Workforce.Workplace.Visitor.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor rules by code.</summary>
public interface IVisitorRuleStore
{
    VisitorRule? Find(string code);
    void Save(VisitorRule item);
    bool Remove(string code);
    List<VisitorRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorRuleStore(Dictionary<string, VisitorRule> items, List<string> order) : IVisitorRuleStore
{
    public Dictionary<string, VisitorRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorRule item)
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

    public List<VisitorRule> List()
    {
        var outValue = new List<VisitorRule>();
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
public sealed class CachedVisitorRuleStore(MemoryVisitorRuleStore inner, VisitorRuleCache cache, int hits, int misses) : IVisitorRuleStore
{
    public MemoryVisitorRuleStore Inner { get; set; } = inner;
    public VisitorRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorRule? Find(string code)
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

    public void Save(VisitorRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor rules. Empty fields match all.</summary>
public sealed record VisitorRuleQuery(string Team, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorRule item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorRule> Apply(List<VisitorRule> items)
    {
        var matched = Collections.Filter(items, (VisitorRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor rule store.</summary>
public static class VisitorRuleStore
{
    public static IVisitorRuleStore NewMemoryVisitorRuleStore()
    {
        return new MemoryVisitorRuleStore(new Dictionary<string, VisitorRule>(), new List<string>());
    }

    public static IVisitorRuleStore NewCachedVisitorRuleStore(MemoryVisitorRuleStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorRuleStore(inner, VisitorRuleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<VisitorRule> FindVisitorRules(IVisitorRuleStore store, VisitorRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
