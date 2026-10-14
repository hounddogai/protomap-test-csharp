namespace Acme.Workforce.Engagement.TownHall.BatchedHallRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary town hall rules by code.</summary>
public interface IPrimaryTownHallRuleStore
{
    PrimaryTownHallRule? Find(string code);
    void Save(PrimaryTownHallRule item);
    bool Remove(string code);
    List<PrimaryTownHallRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryTownHallRuleStore(Dictionary<string, PrimaryTownHallRule> items, List<string> order) : IPrimaryTownHallRuleStore
{
    public Dictionary<string, PrimaryTownHallRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryTownHallRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryTownHallRule item)
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

    public List<PrimaryTownHallRule> List()
    {
        var outValue = new List<PrimaryTownHallRule>();
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
public sealed class CachedPrimaryTownHallRuleStore(MemoryPrimaryTownHallRuleStore inner, PrimaryTownHallRuleCache cache, int hits, int misses) : IPrimaryTownHallRuleStore
{
    public MemoryPrimaryTownHallRuleStore Inner { get; set; } = inner;
    public PrimaryTownHallRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryTownHallRule? Find(string code)
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

    public void Save(PrimaryTownHallRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryTownHallRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary town hall rules. Empty fields match all.</summary>
public sealed record PrimaryTownHallRuleQuery(string Channel, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryTownHallRule item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryTownHallRule> Apply(List<PrimaryTownHallRule> items)
    {
        var matched = Collections.Filter(items, (PrimaryTownHallRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary town hall rule store.</summary>
public static class PrimaryTownHallRuleStore
{
    public static IPrimaryTownHallRuleStore NewMemoryPrimaryTownHallRuleStore()
    {
        return new MemoryPrimaryTownHallRuleStore(new Dictionary<string, PrimaryTownHallRule>(), new List<string>());
    }

    public static IPrimaryTownHallRuleStore NewCachedPrimaryTownHallRuleStore(MemoryPrimaryTownHallRuleStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryTownHallRuleStore(inner, PrimaryTownHallRuleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PrimaryTownHallRule> FindPrimaryTownHallRules(IPrimaryTownHallRuleStore store, PrimaryTownHallRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
