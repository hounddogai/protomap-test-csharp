namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted adjustment templates by code.</summary>
public interface IWeightedAdjustmentTemplateStore
{
    WeightedAdjustmentTemplate? Find(string code);
    void Save(WeightedAdjustmentTemplate item);
    bool Remove(string code);
    List<WeightedAdjustmentTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedAdjustmentTemplateStore(Dictionary<string, WeightedAdjustmentTemplate> items, List<string> order) : IWeightedAdjustmentTemplateStore
{
    public Dictionary<string, WeightedAdjustmentTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedAdjustmentTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedAdjustmentTemplate item)
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

    public List<WeightedAdjustmentTemplate> List()
    {
        var outValue = new List<WeightedAdjustmentTemplate>();
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
public sealed class CachedWeightedAdjustmentTemplateStore(MemoryWeightedAdjustmentTemplateStore inner, WeightedAdjustmentTemplateCache cache, int hits, int misses) : IWeightedAdjustmentTemplateStore
{
    public MemoryWeightedAdjustmentTemplateStore Inner { get; set; } = inner;
    public WeightedAdjustmentTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedAdjustmentTemplate? Find(string code)
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

    public void Save(WeightedAdjustmentTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedAdjustmentTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted adjustment templates. Empty fields match all.</summary>
public sealed record WeightedAdjustmentTemplateQuery(string Department, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedAdjustmentTemplate item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedAdjustmentTemplate> Apply(List<WeightedAdjustmentTemplate> items)
    {
        var matched = Collections.Filter(items, (WeightedAdjustmentTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted adjustment template store.</summary>
public static class WeightedAdjustmentTemplateStore
{
    public static IWeightedAdjustmentTemplateStore NewMemoryWeightedAdjustmentTemplateStore()
    {
        return new MemoryWeightedAdjustmentTemplateStore(
            new Dictionary<string, WeightedAdjustmentTemplate>(),
            new List<string>());
    }

    public static IWeightedAdjustmentTemplateStore NewCachedWeightedAdjustmentTemplateStore(MemoryWeightedAdjustmentTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedAdjustmentTemplateStore(
            inner,
            WeightedAdjustmentTemplateCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<WeightedAdjustmentTemplate> FindWeightedAdjustmentTemplates(IWeightedAdjustmentTemplateStore store, WeightedAdjustmentTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
