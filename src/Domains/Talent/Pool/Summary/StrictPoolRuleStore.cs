namespace Acme.Workforce.Talent.Pool.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict pool rules by code.</summary>
public interface IStrictPoolRuleStore
{
    StrictPoolRule? Find(string code);
    void Save(StrictPoolRule item);
    bool Remove(string code);
    List<StrictPoolRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictPoolRuleStore(Dictionary<string, StrictPoolRule> items, List<string> order) : IStrictPoolRuleStore
{
    public Dictionary<string, StrictPoolRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictPoolRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictPoolRule item)
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

    public List<StrictPoolRule> List()
    {
        var outValue = new List<StrictPoolRule>();
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
public sealed class CachedStrictPoolRuleStore(MemoryStrictPoolRuleStore inner, StrictPoolRuleCache cache, int hits, int misses) : IStrictPoolRuleStore
{
    public MemoryStrictPoolRuleStore Inner { get; set; } = inner;
    public StrictPoolRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictPoolRule? Find(string code)
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

    public void Save(StrictPoolRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictPoolRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict pool rules. Empty fields match all.</summary>
public sealed record StrictPoolRuleQuery(string Region, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(StrictPoolRule item)
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

    public List<StrictPoolRule> Apply(List<StrictPoolRule> items)
    {
        var matched = Collections.Filter(items, (StrictPoolRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict pool rule store.</summary>
public static class StrictPoolRuleStore
{
    public static IStrictPoolRuleStore NewMemoryStrictPoolRuleStore()
    {
        return new MemoryStrictPoolRuleStore(new Dictionary<string, StrictPoolRule>(), new List<string>());
    }

    public static IStrictPoolRuleStore NewCachedStrictPoolRuleStore(MemoryStrictPoolRuleStore inner, int ttl, IClock clock)
    {
        return new CachedStrictPoolRuleStore(inner, StrictPoolRuleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<StrictPoolRule> FindStrictPoolRules(IStrictPoolRuleStore store, StrictPoolRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
