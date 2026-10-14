namespace Acme.Workforce.Compensation.BonusPool.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool rules by code.</summary>
public interface IBonusPoolRuleStore
{
    BonusPoolRule? Find(string code);
    void Save(BonusPoolRule item);
    bool Remove(string code);
    List<BonusPoolRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolRuleStore(Dictionary<string, BonusPoolRule> items, List<string> order) : IBonusPoolRuleStore
{
    public Dictionary<string, BonusPoolRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolRule item)
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

    public List<BonusPoolRule> List()
    {
        var outValue = new List<BonusPoolRule>();
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
public sealed class CachedBonusPoolRuleStore(MemoryBonusPoolRuleStore inner, BonusPoolRuleCache cache, int hits, int misses) : IBonusPoolRuleStore
{
    public MemoryBonusPoolRuleStore Inner { get; set; } = inner;
    public BonusPoolRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolRule? Find(string code)
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

    public void Save(BonusPoolRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool rules. Empty fields match all.</summary>
public sealed record BonusPoolRuleQuery(string Channel, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolRule item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolRule> Apply(List<BonusPoolRule> items)
    {
        var matched = Collections.Filter(items, (BonusPoolRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool rule store.</summary>
public static class BonusPoolRuleStore
{
    public static IBonusPoolRuleStore NewMemoryBonusPoolRuleStore()
    {
        return new MemoryBonusPoolRuleStore(new Dictionary<string, BonusPoolRule>(), new List<string>());
    }

    public static IBonusPoolRuleStore NewCachedBonusPoolRuleStore(MemoryBonusPoolRuleStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolRuleStore(inner, BonusPoolRuleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BonusPoolRule> FindBonusPoolRules(IBonusPoolRuleStore store, BonusPoolRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
