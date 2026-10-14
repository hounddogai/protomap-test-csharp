namespace Acme.Workforce.Payrollops.TaxForm.FormTemplate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form tiers by code.</summary>
public interface ITaxFormTierStore
{
    TaxFormTier? Find(string code);
    void Save(TaxFormTier item);
    bool Remove(string code);
    List<TaxFormTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormTierStore(Dictionary<string, TaxFormTier> items, List<string> order) : ITaxFormTierStore
{
    public Dictionary<string, TaxFormTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormTier item)
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

    public List<TaxFormTier> List()
    {
        var outValue = new List<TaxFormTier>();
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
public sealed class CachedTaxFormTierStore(MemoryTaxFormTierStore inner, TaxFormTierCache cache, int hits, int misses) : ITaxFormTierStore
{
    public MemoryTaxFormTierStore Inner { get; set; } = inner;
    public TaxFormTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormTier? Find(string code)
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

    public void Save(TaxFormTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form tiers. Empty fields match all.</summary>
public sealed record TaxFormTierQuery(string Segment, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormTier item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TaxFormTier> Apply(List<TaxFormTier> items)
    {
        var matched = Collections.Filter(items, (TaxFormTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form tier store.</summary>
public static class TaxFormTierStore
{
    public static ITaxFormTierStore NewMemoryTaxFormTierStore()
    {
        return new MemoryTaxFormTierStore(new Dictionary<string, TaxFormTier>(), new List<string>());
    }

    public static ITaxFormTierStore NewCachedTaxFormTierStore(MemoryTaxFormTierStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormTierStore(inner, TaxFormTierCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TaxFormTier> FindTaxFormTiers(ITaxFormTierStore store, TaxFormTierQuery query)
    {
        return query.Apply(store.List());
    }
}
