namespace Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional return plan items by code.</summary>
public interface IRegionalReturnPlanItemStore
{
    RegionalReturnPlanItem? Find(string code);
    void Save(RegionalReturnPlanItem item);
    bool Remove(string code);
    List<RegionalReturnPlanItem> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalReturnPlanItemStore(Dictionary<string, RegionalReturnPlanItem> items, List<string> order) : IRegionalReturnPlanItemStore
{
    public Dictionary<string, RegionalReturnPlanItem> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalReturnPlanItem? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalReturnPlanItem item)
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

    public List<RegionalReturnPlanItem> List()
    {
        var outValue = new List<RegionalReturnPlanItem>();
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
public sealed class CachedRegionalReturnPlanItemStore(MemoryRegionalReturnPlanItemStore inner, RegionalReturnPlanItemCache cache, int hits, int misses) : IRegionalReturnPlanItemStore
{
    public MemoryRegionalReturnPlanItemStore Inner { get; set; } = inner;
    public RegionalReturnPlanItemCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalReturnPlanItem? Find(string code)
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

    public void Save(RegionalReturnPlanItem item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalReturnPlanItem> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional return plan items. Empty fields match all.</summary>
public sealed record RegionalReturnPlanItemQuery(string Name, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalReturnPlanItem item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalReturnPlanItem> Apply(List<RegionalReturnPlanItem> items)
    {
        var matched = Collections.Filter(items, (RegionalReturnPlanItem candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional return plan item store.</summary>
public static class RegionalReturnPlanItemStore
{
    public static IRegionalReturnPlanItemStore NewMemoryRegionalReturnPlanItemStore()
    {
        return new MemoryRegionalReturnPlanItemStore(
            new Dictionary<string, RegionalReturnPlanItem>(),
            new List<string>());
    }

    public static IRegionalReturnPlanItemStore NewCachedRegionalReturnPlanItemStore(MemoryRegionalReturnPlanItemStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalReturnPlanItemStore(
            inner,
            RegionalReturnPlanItemCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<RegionalReturnPlanItem> FindRegionalReturnPlanItems(IRegionalReturnPlanItemStore store, RegionalReturnPlanItemQuery query)
    {
        return query.Apply(store.List());
    }
}
