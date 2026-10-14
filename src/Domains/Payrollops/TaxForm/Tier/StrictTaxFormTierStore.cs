namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict tax form tiers by code.</summary>
public interface IStrictTaxFormTierStore
{
    StrictTaxFormTier? Find(string code);
    void Save(StrictTaxFormTier item);
    bool Remove(string code);
    List<StrictTaxFormTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictTaxFormTierStore(Dictionary<string, StrictTaxFormTier> items, List<string> order) : IStrictTaxFormTierStore
{
    public Dictionary<string, StrictTaxFormTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictTaxFormTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictTaxFormTier item)
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

    public List<StrictTaxFormTier> List()
    {
        var outValue = new List<StrictTaxFormTier>();
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
public sealed class CachedStrictTaxFormTierStore(MemoryStrictTaxFormTierStore inner, StrictTaxFormTierCache cache, int hits, int misses) : IStrictTaxFormTierStore
{
    public MemoryStrictTaxFormTierStore Inner { get; set; } = inner;
    public StrictTaxFormTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictTaxFormTier? Find(string code)
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

    public void Save(StrictTaxFormTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictTaxFormTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict tax form tiers. Empty fields match all.</summary>
public sealed record StrictTaxFormTierQuery(string Label, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(StrictTaxFormTier item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictTaxFormTier> Apply(List<StrictTaxFormTier> items)
    {
        var matched = Collections.Filter(items, (StrictTaxFormTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict tax form tier store.</summary>
public static class StrictTaxFormTierStore
{
    public static IStrictTaxFormTierStore NewMemoryStrictTaxFormTierStore()
    {
        return new MemoryStrictTaxFormTierStore(new Dictionary<string, StrictTaxFormTier>(), new List<string>());
    }

    public static IStrictTaxFormTierStore NewCachedStrictTaxFormTierStore(MemoryStrictTaxFormTierStore inner, int ttl, IClock clock)
    {
        return new CachedStrictTaxFormTierStore(inner, StrictTaxFormTierCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<StrictTaxFormTier> FindStrictTaxFormTiers(IStrictTaxFormTierStore store, StrictTaxFormTierQuery query)
    {
        return query.Apply(store.List());
    }
}
