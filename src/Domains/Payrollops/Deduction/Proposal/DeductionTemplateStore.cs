namespace Acme.Workforce.Payrollops.Deduction.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction templates by code.</summary>
public interface IDeductionTemplateStore
{
    DeductionTemplate? Find(string code);
    void Save(DeductionTemplate item);
    bool Remove(string code);
    List<DeductionTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionTemplateStore(Dictionary<string, DeductionTemplate> items, List<string> order) : IDeductionTemplateStore
{
    public Dictionary<string, DeductionTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionTemplate item)
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

    public List<DeductionTemplate> List()
    {
        var outValue = new List<DeductionTemplate>();
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
public sealed class CachedDeductionTemplateStore(MemoryDeductionTemplateStore inner, DeductionTemplateCache cache, int hits, int misses) : IDeductionTemplateStore
{
    public MemoryDeductionTemplateStore Inner { get; set; } = inner;
    public DeductionTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionTemplate? Find(string code)
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

    public void Save(DeductionTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction templates. Empty fields match all.</summary>
public sealed record DeductionTemplateQuery(string Label, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionTemplate item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeductionTemplate> Apply(List<DeductionTemplate> items)
    {
        var matched = Collections.Filter(items, (DeductionTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction template store.</summary>
public static class DeductionTemplateStore
{
    public static IDeductionTemplateStore NewMemoryDeductionTemplateStore()
    {
        return new MemoryDeductionTemplateStore(new Dictionary<string, DeductionTemplate>(), new List<string>());
    }

    public static IDeductionTemplateStore NewCachedDeductionTemplateStore(MemoryDeductionTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionTemplateStore(inner, DeductionTemplateCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<DeductionTemplate> FindDeductionTemplates(IDeductionTemplateStore store, DeductionTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
