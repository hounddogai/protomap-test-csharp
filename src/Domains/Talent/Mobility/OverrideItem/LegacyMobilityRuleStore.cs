namespace Acme.Workforce.Talent.Mobility.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy mobility rules by code.</summary>
public interface ILegacyMobilityRuleStore
{
    LegacyMobilityRule? Find(string code);
    void Save(LegacyMobilityRule item);
    bool Remove(string code);
    List<LegacyMobilityRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyMobilityRuleStore(Dictionary<string, LegacyMobilityRule> items, List<string> order) : ILegacyMobilityRuleStore
{
    public Dictionary<string, LegacyMobilityRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyMobilityRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyMobilityRule item)
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

    public List<LegacyMobilityRule> List()
    {
        var outValue = new List<LegacyMobilityRule>();
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
public sealed class CachedLegacyMobilityRuleStore(MemoryLegacyMobilityRuleStore inner, LegacyMobilityRuleCache cache, int hits, int misses) : ILegacyMobilityRuleStore
{
    public MemoryLegacyMobilityRuleStore Inner { get; set; } = inner;
    public LegacyMobilityRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyMobilityRule? Find(string code)
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

    public void Save(LegacyMobilityRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyMobilityRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy mobility rules. Empty fields match all.</summary>
public sealed record LegacyMobilityRuleQuery(string Title, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyMobilityRule item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyMobilityRule> Apply(List<LegacyMobilityRule> items)
    {
        var matched = Collections.Filter(items, (LegacyMobilityRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy mobility rule store.</summary>
public static class LegacyMobilityRuleStore
{
    public static ILegacyMobilityRuleStore NewMemoryLegacyMobilityRuleStore()
    {
        return new MemoryLegacyMobilityRuleStore(new Dictionary<string, LegacyMobilityRule>(), new List<string>());
    }

    public static ILegacyMobilityRuleStore NewCachedLegacyMobilityRuleStore(MemoryLegacyMobilityRuleStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyMobilityRuleStore(inner, LegacyMobilityRuleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LegacyMobilityRule> FindLegacyMobilityRules(ILegacyMobilityRuleStore store, LegacyMobilityRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
