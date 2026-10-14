namespace Acme.Workforce.Talent.Nomination.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered nomination rules by code.</summary>
public interface ITieredNominationRuleStore
{
    TieredNominationRule? Find(string code);
    void Save(TieredNominationRule item);
    bool Remove(string code);
    List<TieredNominationRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredNominationRuleStore(Dictionary<string, TieredNominationRule> items, List<string> order) : ITieredNominationRuleStore
{
    public Dictionary<string, TieredNominationRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredNominationRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredNominationRule item)
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

    public List<TieredNominationRule> List()
    {
        var outValue = new List<TieredNominationRule>();
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
public sealed class CachedTieredNominationRuleStore(MemoryTieredNominationRuleStore inner, TieredNominationRuleCache cache, int hits, int misses) : ITieredNominationRuleStore
{
    public MemoryTieredNominationRuleStore Inner { get; set; } = inner;
    public TieredNominationRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredNominationRule? Find(string code)
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

    public void Save(TieredNominationRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredNominationRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered nomination rules. Empty fields match all.</summary>
public sealed record TieredNominationRuleQuery(string Unit, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(TieredNominationRule item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredNominationRule> Apply(List<TieredNominationRule> items)
    {
        var matched = Collections.Filter(items, (TieredNominationRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered nomination rule store.</summary>
public static class TieredNominationRuleStore
{
    public static ITieredNominationRuleStore NewMemoryTieredNominationRuleStore()
    {
        return new MemoryTieredNominationRuleStore(
            new Dictionary<string, TieredNominationRule>(),
            new List<string>());
    }

    public static ITieredNominationRuleStore NewCachedTieredNominationRuleStore(MemoryTieredNominationRuleStore inner, int ttl, IClock clock)
    {
        return new CachedTieredNominationRuleStore(inner, TieredNominationRuleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<TieredNominationRule> FindTieredNominationRules(ITieredNominationRuleStore store, TieredNominationRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
