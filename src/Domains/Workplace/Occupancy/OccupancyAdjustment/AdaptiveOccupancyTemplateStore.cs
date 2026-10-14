namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive occupancy templates by code.</summary>
public interface IAdaptiveOccupancyTemplateStore
{
    AdaptiveOccupancyTemplate? Find(string code);
    void Save(AdaptiveOccupancyTemplate item);
    bool Remove(string code);
    List<AdaptiveOccupancyTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveOccupancyTemplateStore(Dictionary<string, AdaptiveOccupancyTemplate> items, List<string> order) : IAdaptiveOccupancyTemplateStore
{
    public Dictionary<string, AdaptiveOccupancyTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveOccupancyTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveOccupancyTemplate item)
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

    public List<AdaptiveOccupancyTemplate> List()
    {
        var outValue = new List<AdaptiveOccupancyTemplate>();
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
public sealed class CachedAdaptiveOccupancyTemplateStore(MemoryAdaptiveOccupancyTemplateStore inner, AdaptiveOccupancyTemplateCache cache, int hits, int misses) : IAdaptiveOccupancyTemplateStore
{
    public MemoryAdaptiveOccupancyTemplateStore Inner { get; set; } = inner;
    public AdaptiveOccupancyTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveOccupancyTemplate? Find(string code)
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

    public void Save(AdaptiveOccupancyTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveOccupancyTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive occupancy templates. Empty fields match all.</summary>
public sealed record AdaptiveOccupancyTemplateQuery(string Source, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveOccupancyTemplate item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<AdaptiveOccupancyTemplate> Apply(List<AdaptiveOccupancyTemplate> items)
    {
        var matched = Collections.Filter(items, (AdaptiveOccupancyTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive occupancy template store.</summary>
public static class AdaptiveOccupancyTemplateStore
{
    public static IAdaptiveOccupancyTemplateStore NewMemoryAdaptiveOccupancyTemplateStore()
    {
        return new MemoryAdaptiveOccupancyTemplateStore(
            new Dictionary<string, AdaptiveOccupancyTemplate>(),
            new List<string>());
    }

    public static IAdaptiveOccupancyTemplateStore NewCachedAdaptiveOccupancyTemplateStore(MemoryAdaptiveOccupancyTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveOccupancyTemplateStore(
            inner,
            AdaptiveOccupancyTemplateCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveOccupancyTemplate> FindAdaptiveOccupancyTemplates(IAdaptiveOccupancyTemplateStore store, AdaptiveOccupancyTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
