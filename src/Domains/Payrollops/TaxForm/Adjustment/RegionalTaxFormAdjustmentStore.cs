namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional tax form adjustments by code.</summary>
public interface IRegionalTaxFormAdjustmentStore
{
    RegionalTaxFormAdjustment? Find(string code);
    void Save(RegionalTaxFormAdjustment item);
    bool Remove(string code);
    List<RegionalTaxFormAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalTaxFormAdjustmentStore(Dictionary<string, RegionalTaxFormAdjustment> items, List<string> order) : IRegionalTaxFormAdjustmentStore
{
    public Dictionary<string, RegionalTaxFormAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalTaxFormAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalTaxFormAdjustment item)
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

    public List<RegionalTaxFormAdjustment> List()
    {
        var outValue = new List<RegionalTaxFormAdjustment>();
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
public sealed class CachedRegionalTaxFormAdjustmentStore(MemoryRegionalTaxFormAdjustmentStore inner, RegionalTaxFormAdjustmentCache cache, int hits, int misses) : IRegionalTaxFormAdjustmentStore
{
    public MemoryRegionalTaxFormAdjustmentStore Inner { get; set; } = inner;
    public RegionalTaxFormAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalTaxFormAdjustment? Find(string code)
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

    public void Save(RegionalTaxFormAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalTaxFormAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional tax form adjustments. Empty fields match all.</summary>
public sealed record RegionalTaxFormAdjustmentQuery(string Title, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalTaxFormAdjustment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<RegionalTaxFormAdjustment> Apply(List<RegionalTaxFormAdjustment> items)
    {
        var matched = Collections.Filter(items, (RegionalTaxFormAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional tax form adjustment store.</summary>
public static class RegionalTaxFormAdjustmentStore
{
    public static IRegionalTaxFormAdjustmentStore NewMemoryRegionalTaxFormAdjustmentStore()
    {
        return new MemoryRegionalTaxFormAdjustmentStore(
            new Dictionary<string, RegionalTaxFormAdjustment>(),
            new List<string>());
    }

    public static IRegionalTaxFormAdjustmentStore NewCachedRegionalTaxFormAdjustmentStore(MemoryRegionalTaxFormAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalTaxFormAdjustmentStore(
            inner,
            RegionalTaxFormAdjustmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RegionalTaxFormAdjustment> FindRegionalTaxFormAdjustments(IRegionalTaxFormAdjustmentStore store, RegionalTaxFormAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
