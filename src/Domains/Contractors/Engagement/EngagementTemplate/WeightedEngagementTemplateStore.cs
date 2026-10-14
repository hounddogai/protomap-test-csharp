namespace Acme.Workforce.Contractors.Engagement.EngagementTemplate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted engagement templates by code.</summary>
public interface IWeightedEngagementTemplateStore
{
    WeightedEngagementTemplate? Find(string code);
    void Save(WeightedEngagementTemplate item);
    bool Remove(string code);
    List<WeightedEngagementTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedEngagementTemplateStore(Dictionary<string, WeightedEngagementTemplate> items, List<string> order) : IWeightedEngagementTemplateStore
{
    public Dictionary<string, WeightedEngagementTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedEngagementTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedEngagementTemplate item)
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

    public List<WeightedEngagementTemplate> List()
    {
        var outValue = new List<WeightedEngagementTemplate>();
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
public sealed class CachedWeightedEngagementTemplateStore(MemoryWeightedEngagementTemplateStore inner, WeightedEngagementTemplateCache cache, int hits, int misses) : IWeightedEngagementTemplateStore
{
    public MemoryWeightedEngagementTemplateStore Inner { get; set; } = inner;
    public WeightedEngagementTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedEngagementTemplate? Find(string code)
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

    public void Save(WeightedEngagementTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedEngagementTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted engagement templates. Empty fields match all.</summary>
public sealed record WeightedEngagementTemplateQuery(string Title, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedEngagementTemplate item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedEngagementTemplate> Apply(List<WeightedEngagementTemplate> items)
    {
        var matched = Collections.Filter(items, (WeightedEngagementTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted engagement template store.</summary>
public static class WeightedEngagementTemplateStore
{
    public static IWeightedEngagementTemplateStore NewMemoryWeightedEngagementTemplateStore()
    {
        return new MemoryWeightedEngagementTemplateStore(
            new Dictionary<string, WeightedEngagementTemplate>(),
            new List<string>());
    }

    public static IWeightedEngagementTemplateStore NewCachedWeightedEngagementTemplateStore(MemoryWeightedEngagementTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedEngagementTemplateStore(
            inner,
            WeightedEngagementTemplateCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<WeightedEngagementTemplate> FindWeightedEngagementTemplates(IWeightedEngagementTemplateStore store, WeightedEngagementTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
