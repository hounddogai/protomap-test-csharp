namespace Acme.Workforce.Payrollops.Deduction.DeductionBudget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional deduction templates by code.</summary>
public interface IRegionalDeductionTemplateStore
{
    RegionalDeductionTemplate? Find(string code);
    void Save(RegionalDeductionTemplate item);
    bool Remove(string code);
    List<RegionalDeductionTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalDeductionTemplateStore(Dictionary<string, RegionalDeductionTemplate> items, List<string> order) : IRegionalDeductionTemplateStore
{
    public Dictionary<string, RegionalDeductionTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalDeductionTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalDeductionTemplate item)
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

    public List<RegionalDeductionTemplate> List()
    {
        var outValue = new List<RegionalDeductionTemplate>();
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
public sealed class CachedRegionalDeductionTemplateStore(MemoryRegionalDeductionTemplateStore inner, RegionalDeductionTemplateCache cache, int hits, int misses) : IRegionalDeductionTemplateStore
{
    public MemoryRegionalDeductionTemplateStore Inner { get; set; } = inner;
    public RegionalDeductionTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalDeductionTemplate? Find(string code)
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

    public void Save(RegionalDeductionTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalDeductionTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional deduction templates. Empty fields match all.</summary>
public sealed record RegionalDeductionTemplateQuery(string Name, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalDeductionTemplate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<RegionalDeductionTemplate> Apply(List<RegionalDeductionTemplate> items)
    {
        var matched = Collections.Filter(items, (RegionalDeductionTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional deduction template store.</summary>
public static class RegionalDeductionTemplateStore
{
    public static IRegionalDeductionTemplateStore NewMemoryRegionalDeductionTemplateStore()
    {
        return new MemoryRegionalDeductionTemplateStore(
            new Dictionary<string, RegionalDeductionTemplate>(),
            new List<string>());
    }

    public static IRegionalDeductionTemplateStore NewCachedRegionalDeductionTemplateStore(MemoryRegionalDeductionTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalDeductionTemplateStore(
            inner,
            RegionalDeductionTemplateCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RegionalDeductionTemplate> FindRegionalDeductionTemplates(IRegionalDeductionTemplateStore store, RegionalDeductionTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
