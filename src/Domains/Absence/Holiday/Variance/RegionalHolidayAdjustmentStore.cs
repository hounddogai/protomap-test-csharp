namespace Acme.Workforce.Absence.Holiday.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional holiday adjustments by code.</summary>
public interface IRegionalHolidayAdjustmentStore
{
    RegionalHolidayAdjustment? Find(string code);
    void Save(RegionalHolidayAdjustment item);
    bool Remove(string code);
    List<RegionalHolidayAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalHolidayAdjustmentStore(Dictionary<string, RegionalHolidayAdjustment> items, List<string> order) : IRegionalHolidayAdjustmentStore
{
    public Dictionary<string, RegionalHolidayAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalHolidayAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalHolidayAdjustment item)
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

    public List<RegionalHolidayAdjustment> List()
    {
        var outValue = new List<RegionalHolidayAdjustment>();
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
public sealed class CachedRegionalHolidayAdjustmentStore(MemoryRegionalHolidayAdjustmentStore inner, RegionalHolidayAdjustmentCache cache, int hits, int misses) : IRegionalHolidayAdjustmentStore
{
    public MemoryRegionalHolidayAdjustmentStore Inner { get; set; } = inner;
    public RegionalHolidayAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalHolidayAdjustment? Find(string code)
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

    public void Save(RegionalHolidayAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalHolidayAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional holiday adjustments. Empty fields match all.</summary>
public sealed record RegionalHolidayAdjustmentQuery(string Locale, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalHolidayAdjustment item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<RegionalHolidayAdjustment> Apply(List<RegionalHolidayAdjustment> items)
    {
        var matched = Collections.Filter(items, (RegionalHolidayAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional holiday adjustment store.</summary>
public static class RegionalHolidayAdjustmentStore
{
    public static IRegionalHolidayAdjustmentStore NewMemoryRegionalHolidayAdjustmentStore()
    {
        return new MemoryRegionalHolidayAdjustmentStore(
            new Dictionary<string, RegionalHolidayAdjustment>(),
            new List<string>());
    }

    public static IRegionalHolidayAdjustmentStore NewCachedRegionalHolidayAdjustmentStore(MemoryRegionalHolidayAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalHolidayAdjustmentStore(
            inner,
            RegionalHolidayAdjustmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RegionalHolidayAdjustment> FindRegionalHolidayAdjustments(IRegionalHolidayAdjustmentStore store, RegionalHolidayAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
