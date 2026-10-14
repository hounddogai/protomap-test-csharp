namespace Acme.Workforce.Orgdesign.CostModel.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps cost model rollups by code.</summary>
public interface ICostModelRollupStore
{
    CostModelRollup? Find(string code);
    void Save(CostModelRollup item);
    bool Remove(string code);
    List<CostModelRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCostModelRollupStore(Dictionary<string, CostModelRollup> items, List<string> order) : ICostModelRollupStore
{
    public Dictionary<string, CostModelRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CostModelRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CostModelRollup item)
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

    public List<CostModelRollup> List()
    {
        var outValue = new List<CostModelRollup>();
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
public sealed class CachedCostModelRollupStore(MemoryCostModelRollupStore inner, CostModelRollupCache cache, int hits, int misses) : ICostModelRollupStore
{
    public MemoryCostModelRollupStore Inner { get; set; } = inner;
    public CostModelRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CostModelRollup? Find(string code)
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

    public void Save(CostModelRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CostModelRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects cost model rollups. Empty fields match all.</summary>
public sealed record CostModelRollupQuery(string Channel, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(CostModelRollup item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CostModelRollup> Apply(List<CostModelRollup> items)
    {
        var matched = Collections.Filter(items, (CostModelRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of cost model rollup store.</summary>
public static class CostModelRollupStore
{
    public static ICostModelRollupStore NewMemoryCostModelRollupStore()
    {
        return new MemoryCostModelRollupStore(new Dictionary<string, CostModelRollup>(), new List<string>());
    }

    public static ICostModelRollupStore NewCachedCostModelRollupStore(MemoryCostModelRollupStore inner, int ttl, IClock clock)
    {
        return new CachedCostModelRollupStore(inner, CostModelRollupCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<CostModelRollup> FindCostModelRollups(ICostModelRollupStore store, CostModelRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
