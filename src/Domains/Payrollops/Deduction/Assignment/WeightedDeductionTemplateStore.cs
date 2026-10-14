namespace Acme.Workforce.Payrollops.Deduction.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted deduction templates by code.</summary>
public interface IWeightedDeductionTemplateStore
{
    WeightedDeductionTemplate? Find(string code);
    void Save(WeightedDeductionTemplate item);
    bool Remove(string code);
    List<WeightedDeductionTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedDeductionTemplateStore(Dictionary<string, WeightedDeductionTemplate> items, List<string> order) : IWeightedDeductionTemplateStore
{
    public Dictionary<string, WeightedDeductionTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedDeductionTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedDeductionTemplate item)
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

    public List<WeightedDeductionTemplate> List()
    {
        var outValue = new List<WeightedDeductionTemplate>();
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
public sealed class CachedWeightedDeductionTemplateStore(MemoryWeightedDeductionTemplateStore inner, WeightedDeductionTemplateCache cache, int hits, int misses) : IWeightedDeductionTemplateStore
{
    public MemoryWeightedDeductionTemplateStore Inner { get; set; } = inner;
    public WeightedDeductionTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedDeductionTemplate? Find(string code)
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

    public void Save(WeightedDeductionTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedDeductionTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted deduction templates. Empty fields match all.</summary>
public sealed record WeightedDeductionTemplateQuery(string Label, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedDeductionTemplate item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedDeductionTemplate> Apply(List<WeightedDeductionTemplate> items)
    {
        var matched = Collections.Filter(items, (WeightedDeductionTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted deduction template store.</summary>
public static class WeightedDeductionTemplateStore
{
    public static IWeightedDeductionTemplateStore NewMemoryWeightedDeductionTemplateStore()
    {
        return new MemoryWeightedDeductionTemplateStore(
            new Dictionary<string, WeightedDeductionTemplate>(),
            new List<string>());
    }

    public static IWeightedDeductionTemplateStore NewCachedWeightedDeductionTemplateStore(MemoryWeightedDeductionTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedDeductionTemplateStore(
            inner,
            WeightedDeductionTemplateCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<WeightedDeductionTemplate> FindWeightedDeductionTemplates(IWeightedDeductionTemplateStore store, WeightedDeductionTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
