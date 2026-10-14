namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps cost model bundles by code.</summary>
public interface ICostModelBundleStore
{
    CostModelBundle? Find(string code);
    void Save(CostModelBundle item);
    bool Remove(string code);
    List<CostModelBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCostModelBundleStore(Dictionary<string, CostModelBundle> items, List<string> order) : ICostModelBundleStore
{
    public Dictionary<string, CostModelBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CostModelBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CostModelBundle item)
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

    public List<CostModelBundle> List()
    {
        var outValue = new List<CostModelBundle>();
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
public sealed class CachedCostModelBundleStore(MemoryCostModelBundleStore inner, CostModelBundleCache cache, int hits, int misses) : ICostModelBundleStore
{
    public MemoryCostModelBundleStore Inner { get; set; } = inner;
    public CostModelBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CostModelBundle? Find(string code)
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

    public void Save(CostModelBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CostModelBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects cost model bundles. Empty fields match all.</summary>
public sealed record CostModelBundleQuery(string Label, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(CostModelBundle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<CostModelBundle> Apply(List<CostModelBundle> items)
    {
        var matched = Collections.Filter(items, (CostModelBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of cost model bundle store.</summary>
public static class CostModelBundleStore
{
    public static ICostModelBundleStore NewMemoryCostModelBundleStore()
    {
        return new MemoryCostModelBundleStore(new Dictionary<string, CostModelBundle>(), new List<string>());
    }

    public static ICostModelBundleStore NewCachedCostModelBundleStore(MemoryCostModelBundleStore inner, int ttl, IClock clock)
    {
        return new CachedCostModelBundleStore(inner, CostModelBundleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<CostModelBundle> FindCostModelBundles(ICostModelBundleStore store, CostModelBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
