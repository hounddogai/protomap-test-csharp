namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient nomination rules by code.</summary>
public interface ILenientNominationRuleStore
{
    LenientNominationRule? Find(string code);
    void Save(LenientNominationRule item);
    bool Remove(string code);
    List<LenientNominationRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientNominationRuleStore(Dictionary<string, LenientNominationRule> items, List<string> order) : ILenientNominationRuleStore
{
    public Dictionary<string, LenientNominationRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientNominationRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientNominationRule item)
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

    public List<LenientNominationRule> List()
    {
        var outValue = new List<LenientNominationRule>();
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
public sealed class CachedLenientNominationRuleStore(MemoryLenientNominationRuleStore inner, LenientNominationRuleCache cache, int hits, int misses) : ILenientNominationRuleStore
{
    public MemoryLenientNominationRuleStore Inner { get; set; } = inner;
    public LenientNominationRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientNominationRule? Find(string code)
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

    public void Save(LenientNominationRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientNominationRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient nomination rules. Empty fields match all.</summary>
public sealed record LenientNominationRuleQuery(string Name, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(LenientNominationRule item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientNominationRule> Apply(List<LenientNominationRule> items)
    {
        var matched = Collections.Filter(items, (LenientNominationRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient nomination rule store.</summary>
public static class LenientNominationRuleStore
{
    public static ILenientNominationRuleStore NewMemoryLenientNominationRuleStore()
    {
        return new MemoryLenientNominationRuleStore(
            new Dictionary<string, LenientNominationRule>(),
            new List<string>());
    }

    public static ILenientNominationRuleStore NewCachedLenientNominationRuleStore(MemoryLenientNominationRuleStore inner, int ttl, IClock clock)
    {
        return new CachedLenientNominationRuleStore(inner, LenientNominationRuleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LenientNominationRule> FindLenientNominationRules(ILenientNominationRuleStore store, LenientNominationRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
