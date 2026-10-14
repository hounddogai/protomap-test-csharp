namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps cost model overrides by code.</summary>
public interface ICostModelOverrideStore
{
    CostModelOverride? Find(string code);
    void Save(CostModelOverride item);
    bool Remove(string code);
    List<CostModelOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCostModelOverrideStore(Dictionary<string, CostModelOverride> items, List<string> order) : ICostModelOverrideStore
{
    public Dictionary<string, CostModelOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CostModelOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CostModelOverride item)
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

    public List<CostModelOverride> List()
    {
        var outValue = new List<CostModelOverride>();
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
public sealed class CachedCostModelOverrideStore(MemoryCostModelOverrideStore inner, CostModelOverrideCache cache, int hits, int misses) : ICostModelOverrideStore
{
    public MemoryCostModelOverrideStore Inner { get; set; } = inner;
    public CostModelOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CostModelOverride? Find(string code)
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

    public void Save(CostModelOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CostModelOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects cost model overrides. Empty fields match all.</summary>
public sealed record CostModelOverrideQuery(string Locale, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(CostModelOverride item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CostModelOverride> Apply(List<CostModelOverride> items)
    {
        var matched = Collections.Filter(items, (CostModelOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of cost model override store.</summary>
public static class CostModelOverrideStore
{
    public static ICostModelOverrideStore NewMemoryCostModelOverrideStore()
    {
        return new MemoryCostModelOverrideStore(new Dictionary<string, CostModelOverride>(), new List<string>());
    }

    public static ICostModelOverrideStore NewCachedCostModelOverrideStore(MemoryCostModelOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedCostModelOverrideStore(inner, CostModelOverrideCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CostModelOverride> FindCostModelOverrides(ICostModelOverrideStore store, CostModelOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
