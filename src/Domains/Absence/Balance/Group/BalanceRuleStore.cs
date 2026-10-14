namespace Acme.Workforce.Absence.Balance.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance rules by code.</summary>
public interface IBalanceRuleStore
{
    BalanceRule? Find(string code);
    void Save(BalanceRule item);
    bool Remove(string code);
    List<BalanceRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalanceRuleStore(Dictionary<string, BalanceRule> items, List<string> order) : IBalanceRuleStore
{
    public Dictionary<string, BalanceRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalanceRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalanceRule item)
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

    public List<BalanceRule> List()
    {
        var outValue = new List<BalanceRule>();
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
public sealed class CachedBalanceRuleStore(MemoryBalanceRuleStore inner, BalanceRuleCache cache, int hits, int misses) : IBalanceRuleStore
{
    public MemoryBalanceRuleStore Inner { get; set; } = inner;
    public BalanceRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalanceRule? Find(string code)
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

    public void Save(BalanceRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalanceRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance rules. Empty fields match all.</summary>
public sealed record BalanceRuleQuery(string Region, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(BalanceRule item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BalanceRule> Apply(List<BalanceRule> items)
    {
        var matched = Collections.Filter(items, (BalanceRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance rule store.</summary>
public static class BalanceRuleStore
{
    public static IBalanceRuleStore NewMemoryBalanceRuleStore()
    {
        return new MemoryBalanceRuleStore(new Dictionary<string, BalanceRule>(), new List<string>());
    }

    public static IBalanceRuleStore NewCachedBalanceRuleStore(MemoryBalanceRuleStore inner, int ttl, IClock clock)
    {
        return new CachedBalanceRuleStore(inner, BalanceRuleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BalanceRule> FindBalanceRules(IBalanceRuleStore store, BalanceRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
