namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary accrual tiers by code.</summary>
public interface IPrimaryAccrualTierStore
{
    PrimaryAccrualTier? Find(string code);
    void Save(PrimaryAccrualTier item);
    bool Remove(string code);
    List<PrimaryAccrualTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryAccrualTierStore(Dictionary<string, PrimaryAccrualTier> items, List<string> order) : IPrimaryAccrualTierStore
{
    public Dictionary<string, PrimaryAccrualTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryAccrualTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryAccrualTier item)
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

    public List<PrimaryAccrualTier> List()
    {
        var outValue = new List<PrimaryAccrualTier>();
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
public sealed class CachedPrimaryAccrualTierStore(MemoryPrimaryAccrualTierStore inner, PrimaryAccrualTierCache cache, int hits, int misses) : IPrimaryAccrualTierStore
{
    public MemoryPrimaryAccrualTierStore Inner { get; set; } = inner;
    public PrimaryAccrualTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryAccrualTier? Find(string code)
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

    public void Save(PrimaryAccrualTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryAccrualTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary accrual tiers. Empty fields match all.</summary>
public sealed record PrimaryAccrualTierQuery(string Category, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryAccrualTier item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryAccrualTier> Apply(List<PrimaryAccrualTier> items)
    {
        var matched = Collections.Filter(items, (PrimaryAccrualTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary accrual tier store.</summary>
public static class PrimaryAccrualTierStore
{
    public static IPrimaryAccrualTierStore NewMemoryPrimaryAccrualTierStore()
    {
        return new MemoryPrimaryAccrualTierStore(new Dictionary<string, PrimaryAccrualTier>(), new List<string>());
    }

    public static IPrimaryAccrualTierStore NewCachedPrimaryAccrualTierStore(MemoryPrimaryAccrualTierStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryAccrualTierStore(inner, PrimaryAccrualTierCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PrimaryAccrualTier> FindPrimaryAccrualTiers(IPrimaryAccrualTierStore store, PrimaryAccrualTierQuery query)
    {
        return query.Apply(store.List());
    }
}
