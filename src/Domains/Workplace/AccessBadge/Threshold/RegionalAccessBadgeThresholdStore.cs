namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional access badge thresholds by code.</summary>
public interface IRegionalAccessBadgeThresholdStore
{
    RegionalAccessBadgeThreshold? Find(string code);
    void Save(RegionalAccessBadgeThreshold item);
    bool Remove(string code);
    List<RegionalAccessBadgeThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalAccessBadgeThresholdStore(Dictionary<string, RegionalAccessBadgeThreshold> items, List<string> order) : IRegionalAccessBadgeThresholdStore
{
    public Dictionary<string, RegionalAccessBadgeThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalAccessBadgeThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalAccessBadgeThreshold item)
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

    public List<RegionalAccessBadgeThreshold> List()
    {
        var outValue = new List<RegionalAccessBadgeThreshold>();
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
public sealed class CachedRegionalAccessBadgeThresholdStore(MemoryRegionalAccessBadgeThresholdStore inner, RegionalAccessBadgeThresholdCache cache, int hits, int misses) : IRegionalAccessBadgeThresholdStore
{
    public MemoryRegionalAccessBadgeThresholdStore Inner { get; set; } = inner;
    public RegionalAccessBadgeThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalAccessBadgeThreshold? Find(string code)
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

    public void Save(RegionalAccessBadgeThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalAccessBadgeThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional access badge thresholds. Empty fields match all.</summary>
public sealed record RegionalAccessBadgeThresholdQuery(string Unit, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalAccessBadgeThreshold item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<RegionalAccessBadgeThreshold> Apply(List<RegionalAccessBadgeThreshold> items)
    {
        var matched = Collections.Filter(items, (RegionalAccessBadgeThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional access badge threshold store.</summary>
public static class RegionalAccessBadgeThresholdStore
{
    public static IRegionalAccessBadgeThresholdStore NewMemoryRegionalAccessBadgeThresholdStore()
    {
        return new MemoryRegionalAccessBadgeThresholdStore(
            new Dictionary<string, RegionalAccessBadgeThreshold>(),
            new List<string>());
    }

    public static IRegionalAccessBadgeThresholdStore NewCachedRegionalAccessBadgeThresholdStore(MemoryRegionalAccessBadgeThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalAccessBadgeThresholdStore(
            inner,
            RegionalAccessBadgeThresholdCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<RegionalAccessBadgeThreshold> FindRegionalAccessBadgeThresholds(IRegionalAccessBadgeThresholdStore store, RegionalAccessBadgeThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
