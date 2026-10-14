namespace Acme.Workforce.Absence.Accrual.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual tiers by code.</summary>
public interface IAccrualTierStore
{
    AccrualTier? Find(string code);
    void Save(AccrualTier item);
    bool Remove(string code);
    List<AccrualTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualTierStore(Dictionary<string, AccrualTier> items, List<string> order) : IAccrualTierStore
{
    public Dictionary<string, AccrualTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualTier item)
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

    public List<AccrualTier> List()
    {
        var outValue = new List<AccrualTier>();
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
public sealed class CachedAccrualTierStore(MemoryAccrualTierStore inner, AccrualTierCache cache, int hits, int misses) : IAccrualTierStore
{
    public MemoryAccrualTierStore Inner { get; set; } = inner;
    public AccrualTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualTier? Find(string code)
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

    public void Save(AccrualTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual tiers. Empty fields match all.</summary>
public sealed record AccrualTierQuery(string Unit, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualTier item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<AccrualTier> Apply(List<AccrualTier> items)
    {
        var matched = Collections.Filter(items, (AccrualTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual tier store.</summary>
public static class AccrualTierStore
{
    public static IAccrualTierStore NewMemoryAccrualTierStore()
    {
        return new MemoryAccrualTierStore(new Dictionary<string, AccrualTier>(), new List<string>());
    }

    public static IAccrualTierStore NewCachedAccrualTierStore(MemoryAccrualTierStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualTierStore(inner, AccrualTierCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AccrualTier> FindAccrualTiers(IAccrualTierStore store, AccrualTierQuery query)
    {
        return query.Apply(store.List());
    }
}
