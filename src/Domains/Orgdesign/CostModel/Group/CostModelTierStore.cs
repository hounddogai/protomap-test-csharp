namespace Acme.Workforce.Orgdesign.CostModel.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps cost model tiers by code.</summary>
public interface ICostModelTierStore
{
    CostModelTier? Find(string code);
    void Save(CostModelTier item);
    bool Remove(string code);
    List<CostModelTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCostModelTierStore(Dictionary<string, CostModelTier> items, List<string> order) : ICostModelTierStore
{
    public Dictionary<string, CostModelTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CostModelTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CostModelTier item)
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

    public List<CostModelTier> List()
    {
        var outValue = new List<CostModelTier>();
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
public sealed class CachedCostModelTierStore(MemoryCostModelTierStore inner, CostModelTierCache cache, int hits, int misses) : ICostModelTierStore
{
    public MemoryCostModelTierStore Inner { get; set; } = inner;
    public CostModelTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CostModelTier? Find(string code)
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

    public void Save(CostModelTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CostModelTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects cost model tiers. Empty fields match all.</summary>
public sealed record CostModelTierQuery(string Channel, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(CostModelTier item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CostModelTier> Apply(List<CostModelTier> items)
    {
        var matched = Collections.Filter(items, (CostModelTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of cost model tier store.</summary>
public static class CostModelTierStore
{
    public static ICostModelTierStore NewMemoryCostModelTierStore()
    {
        return new MemoryCostModelTierStore(new Dictionary<string, CostModelTier>(), new List<string>());
    }

    public static ICostModelTierStore NewCachedCostModelTierStore(MemoryCostModelTierStore inner, int ttl, IClock clock)
    {
        return new CachedCostModelTierStore(inner, CostModelTierCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CostModelTier> FindCostModelTiers(ICostModelTierStore store, CostModelTierQuery query)
    {
        return query.Apply(store.List());
    }
}
