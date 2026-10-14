namespace Acme.Workforce.Contractors.Rate.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive rate templates by code.</summary>
public interface IAdaptiveRateTemplateStore
{
    AdaptiveRateTemplate? Find(string code);
    void Save(AdaptiveRateTemplate item);
    bool Remove(string code);
    List<AdaptiveRateTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveRateTemplateStore(Dictionary<string, AdaptiveRateTemplate> items, List<string> order) : IAdaptiveRateTemplateStore
{
    public Dictionary<string, AdaptiveRateTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveRateTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveRateTemplate item)
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

    public List<AdaptiveRateTemplate> List()
    {
        var outValue = new List<AdaptiveRateTemplate>();
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
public sealed class CachedAdaptiveRateTemplateStore(MemoryAdaptiveRateTemplateStore inner, AdaptiveRateTemplateCache cache, int hits, int misses) : IAdaptiveRateTemplateStore
{
    public MemoryAdaptiveRateTemplateStore Inner { get; set; } = inner;
    public AdaptiveRateTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveRateTemplate? Find(string code)
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

    public void Save(AdaptiveRateTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveRateTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive rate templates. Empty fields match all.</summary>
public sealed record AdaptiveRateTemplateQuery(string Channel, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveRateTemplate item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<AdaptiveRateTemplate> Apply(List<AdaptiveRateTemplate> items)
    {
        var matched = Collections.Filter(items, (AdaptiveRateTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive rate template store.</summary>
public static class AdaptiveRateTemplateStore
{
    public static IAdaptiveRateTemplateStore NewMemoryAdaptiveRateTemplateStore()
    {
        return new MemoryAdaptiveRateTemplateStore(
            new Dictionary<string, AdaptiveRateTemplate>(),
            new List<string>());
    }

    public static IAdaptiveRateTemplateStore NewCachedAdaptiveRateTemplateStore(MemoryAdaptiveRateTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveRateTemplateStore(inner, AdaptiveRateTemplateCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AdaptiveRateTemplate> FindAdaptiveRateTemplates(IAdaptiveRateTemplateStore store, AdaptiveRateTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
