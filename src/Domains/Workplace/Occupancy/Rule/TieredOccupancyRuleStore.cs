namespace Acme.Workforce.Workplace.Occupancy.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered occupancy rules by code.</summary>
public interface ITieredOccupancyRuleStore
{
    TieredOccupancyRule? Find(string code);
    void Save(TieredOccupancyRule item);
    bool Remove(string code);
    List<TieredOccupancyRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredOccupancyRuleStore(Dictionary<string, TieredOccupancyRule> items, List<string> order) : ITieredOccupancyRuleStore
{
    public Dictionary<string, TieredOccupancyRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredOccupancyRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredOccupancyRule item)
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

    public List<TieredOccupancyRule> List()
    {
        var outValue = new List<TieredOccupancyRule>();
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
public sealed class CachedTieredOccupancyRuleStore(MemoryTieredOccupancyRuleStore inner, TieredOccupancyRuleCache cache, int hits, int misses) : ITieredOccupancyRuleStore
{
    public MemoryTieredOccupancyRuleStore Inner { get; set; } = inner;
    public TieredOccupancyRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredOccupancyRule? Find(string code)
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

    public void Save(TieredOccupancyRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredOccupancyRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered occupancy rules. Empty fields match all.</summary>
public sealed record TieredOccupancyRuleQuery(string Locale, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(TieredOccupancyRule item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<TieredOccupancyRule> Apply(List<TieredOccupancyRule> items)
    {
        var matched = Collections.Filter(items, (TieredOccupancyRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered occupancy rule store.</summary>
public static class TieredOccupancyRuleStore
{
    public static ITieredOccupancyRuleStore NewMemoryTieredOccupancyRuleStore()
    {
        return new MemoryTieredOccupancyRuleStore(new Dictionary<string, TieredOccupancyRule>(), new List<string>());
    }

    public static ITieredOccupancyRuleStore NewCachedTieredOccupancyRuleStore(MemoryTieredOccupancyRuleStore inner, int ttl, IClock clock)
    {
        return new CachedTieredOccupancyRuleStore(inner, TieredOccupancyRuleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TieredOccupancyRule> FindTieredOccupancyRules(ITieredOccupancyRuleStore store, TieredOccupancyRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
