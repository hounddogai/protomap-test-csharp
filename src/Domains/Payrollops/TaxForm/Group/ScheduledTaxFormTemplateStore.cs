namespace Acme.Workforce.Payrollops.TaxForm.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled tax form templates by code.</summary>
public interface IScheduledTaxFormTemplateStore
{
    ScheduledTaxFormTemplate? Find(string code);
    void Save(ScheduledTaxFormTemplate item);
    bool Remove(string code);
    List<ScheduledTaxFormTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledTaxFormTemplateStore(Dictionary<string, ScheduledTaxFormTemplate> items, List<string> order) : IScheduledTaxFormTemplateStore
{
    public Dictionary<string, ScheduledTaxFormTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledTaxFormTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledTaxFormTemplate item)
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

    public List<ScheduledTaxFormTemplate> List()
    {
        var outValue = new List<ScheduledTaxFormTemplate>();
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
public sealed class CachedScheduledTaxFormTemplateStore(MemoryScheduledTaxFormTemplateStore inner, ScheduledTaxFormTemplateCache cache, int hits, int misses) : IScheduledTaxFormTemplateStore
{
    public MemoryScheduledTaxFormTemplateStore Inner { get; set; } = inner;
    public ScheduledTaxFormTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledTaxFormTemplate? Find(string code)
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

    public void Save(ScheduledTaxFormTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledTaxFormTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled tax form templates. Empty fields match all.</summary>
public sealed record ScheduledTaxFormTemplateQuery(string Region, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledTaxFormTemplate item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledTaxFormTemplate> Apply(List<ScheduledTaxFormTemplate> items)
    {
        var matched = Collections.Filter(items, (ScheduledTaxFormTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled tax form template store.</summary>
public static class ScheduledTaxFormTemplateStore
{
    public static IScheduledTaxFormTemplateStore NewMemoryScheduledTaxFormTemplateStore()
    {
        return new MemoryScheduledTaxFormTemplateStore(
            new Dictionary<string, ScheduledTaxFormTemplate>(),
            new List<string>());
    }

    public static IScheduledTaxFormTemplateStore NewCachedScheduledTaxFormTemplateStore(MemoryScheduledTaxFormTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledTaxFormTemplateStore(
            inner,
            ScheduledTaxFormTemplateCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledTaxFormTemplate> FindScheduledTaxFormTemplates(IScheduledTaxFormTemplateStore store, ScheduledTaxFormTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
