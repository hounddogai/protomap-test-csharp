namespace Acme.Workforce.Talent.Mobility.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility rules by code.</summary>
public interface IMobilityRuleStore
{
    MobilityRule? Find(string code);
    void Save(MobilityRule item);
    bool Remove(string code);
    List<MobilityRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilityRuleStore(Dictionary<string, MobilityRule> items, List<string> order) : IMobilityRuleStore
{
    public Dictionary<string, MobilityRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilityRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilityRule item)
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

    public List<MobilityRule> List()
    {
        var outValue = new List<MobilityRule>();
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
public sealed class CachedMobilityRuleStore(MemoryMobilityRuleStore inner, MobilityRuleCache cache, int hits, int misses) : IMobilityRuleStore
{
    public MemoryMobilityRuleStore Inner { get; set; } = inner;
    public MobilityRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilityRule? Find(string code)
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

    public void Save(MobilityRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilityRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility rules. Empty fields match all.</summary>
public sealed record MobilityRuleQuery(string Category, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(MobilityRule item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MobilityRule> Apply(List<MobilityRule> items)
    {
        var matched = Collections.Filter(items, (MobilityRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility rule store.</summary>
public static class MobilityRuleStore
{
    public static IMobilityRuleStore NewMemoryMobilityRuleStore()
    {
        return new MemoryMobilityRuleStore(new Dictionary<string, MobilityRule>(), new List<string>());
    }

    public static IMobilityRuleStore NewCachedMobilityRuleStore(MemoryMobilityRuleStore inner, int ttl, IClock clock)
    {
        return new CachedMobilityRuleStore(inner, MobilityRuleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<MobilityRule> FindMobilityRules(IMobilityRuleStore store, MobilityRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
