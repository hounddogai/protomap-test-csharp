namespace Acme.Workforce.Payrollops.Deduction.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction tiers by code.</summary>
public interface IDeductionTierStore
{
    DeductionTier? Find(string code);
    void Save(DeductionTier item);
    bool Remove(string code);
    List<DeductionTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionTierStore(Dictionary<string, DeductionTier> items, List<string> order) : IDeductionTierStore
{
    public Dictionary<string, DeductionTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionTier item)
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

    public List<DeductionTier> List()
    {
        var outValue = new List<DeductionTier>();
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
public sealed class CachedDeductionTierStore(MemoryDeductionTierStore inner, DeductionTierCache cache, int hits, int misses) : IDeductionTierStore
{
    public MemoryDeductionTierStore Inner { get; set; } = inner;
    public DeductionTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionTier? Find(string code)
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

    public void Save(DeductionTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction tiers. Empty fields match all.</summary>
public sealed record DeductionTierQuery(string Title, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionTier item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<DeductionTier> Apply(List<DeductionTier> items)
    {
        var matched = Collections.Filter(items, (DeductionTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction tier store.</summary>
public static class DeductionTierStore
{
    public static IDeductionTierStore NewMemoryDeductionTierStore()
    {
        return new MemoryDeductionTierStore(new Dictionary<string, DeductionTier>(), new List<string>());
    }

    public static IDeductionTierStore NewCachedDeductionTierStore(MemoryDeductionTierStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionTierStore(inner, DeductionTierCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<DeductionTier> FindDeductionTiers(IDeductionTierStore store, DeductionTierQuery query)
    {
        return query.Apply(store.List());
    }
}
