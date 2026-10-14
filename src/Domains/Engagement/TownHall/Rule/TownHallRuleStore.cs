namespace Acme.Workforce.Engagement.TownHall.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall rules by code.</summary>
public interface ITownHallRuleStore
{
    TownHallRule? Find(string code);
    void Save(TownHallRule item);
    bool Remove(string code);
    List<TownHallRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallRuleStore(Dictionary<string, TownHallRule> items, List<string> order) : ITownHallRuleStore
{
    public Dictionary<string, TownHallRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallRule item)
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

    public List<TownHallRule> List()
    {
        var outValue = new List<TownHallRule>();
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
public sealed class CachedTownHallRuleStore(MemoryTownHallRuleStore inner, TownHallRuleCache cache, int hits, int misses) : ITownHallRuleStore
{
    public MemoryTownHallRuleStore Inner { get; set; } = inner;
    public TownHallRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallRule? Find(string code)
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

    public void Save(TownHallRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall rules. Empty fields match all.</summary>
public sealed record TownHallRuleQuery(string Name, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallRule item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<TownHallRule> Apply(List<TownHallRule> items)
    {
        var matched = Collections.Filter(items, (TownHallRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall rule store.</summary>
public static class TownHallRuleStore
{
    public static ITownHallRuleStore NewMemoryTownHallRuleStore()
    {
        return new MemoryTownHallRuleStore(new Dictionary<string, TownHallRule>(), new List<string>());
    }

    public static ITownHallRuleStore NewCachedTownHallRuleStore(MemoryTownHallRuleStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallRuleStore(inner, TownHallRuleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TownHallRule> FindTownHallRules(ITownHallRuleStore store, TownHallRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
