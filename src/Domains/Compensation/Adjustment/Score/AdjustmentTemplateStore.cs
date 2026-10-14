namespace Acme.Workforce.Compensation.Adjustment.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment templates by code.</summary>
public interface IAdjustmentTemplateStore
{
    AdjustmentTemplate? Find(string code);
    void Save(AdjustmentTemplate item);
    bool Remove(string code);
    List<AdjustmentTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentTemplateStore(Dictionary<string, AdjustmentTemplate> items, List<string> order) : IAdjustmentTemplateStore
{
    public Dictionary<string, AdjustmentTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentTemplate item)
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

    public List<AdjustmentTemplate> List()
    {
        var outValue = new List<AdjustmentTemplate>();
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
public sealed class CachedAdjustmentTemplateStore(MemoryAdjustmentTemplateStore inner, AdjustmentTemplateCache cache, int hits, int misses) : IAdjustmentTemplateStore
{
    public MemoryAdjustmentTemplateStore Inner { get; set; } = inner;
    public AdjustmentTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentTemplate? Find(string code)
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

    public void Save(AdjustmentTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment templates. Empty fields match all.</summary>
public sealed record AdjustmentTemplateQuery(string Name, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentTemplate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdjustmentTemplate> Apply(List<AdjustmentTemplate> items)
    {
        var matched = Collections.Filter(items, (AdjustmentTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment template store.</summary>
public static class AdjustmentTemplateStore
{
    public static IAdjustmentTemplateStore NewMemoryAdjustmentTemplateStore()
    {
        return new MemoryAdjustmentTemplateStore(new Dictionary<string, AdjustmentTemplate>(), new List<string>());
    }

    public static IAdjustmentTemplateStore NewCachedAdjustmentTemplateStore(MemoryAdjustmentTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentTemplateStore(inner, AdjustmentTemplateCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AdjustmentTemplate> FindAdjustmentTemplates(IAdjustmentTemplateStore store, AdjustmentTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
