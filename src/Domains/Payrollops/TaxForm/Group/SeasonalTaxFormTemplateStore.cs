namespace Acme.Workforce.Payrollops.TaxForm.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal tax form templates by code.</summary>
public interface ISeasonalTaxFormTemplateStore
{
    SeasonalTaxFormTemplate? Find(string code);
    void Save(SeasonalTaxFormTemplate item);
    bool Remove(string code);
    List<SeasonalTaxFormTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalTaxFormTemplateStore(Dictionary<string, SeasonalTaxFormTemplate> items, List<string> order) : ISeasonalTaxFormTemplateStore
{
    public Dictionary<string, SeasonalTaxFormTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalTaxFormTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalTaxFormTemplate item)
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

    public List<SeasonalTaxFormTemplate> List()
    {
        var outValue = new List<SeasonalTaxFormTemplate>();
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
public sealed class CachedSeasonalTaxFormTemplateStore(MemorySeasonalTaxFormTemplateStore inner, SeasonalTaxFormTemplateCache cache, int hits, int misses) : ISeasonalTaxFormTemplateStore
{
    public MemorySeasonalTaxFormTemplateStore Inner { get; set; } = inner;
    public SeasonalTaxFormTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalTaxFormTemplate? Find(string code)
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

    public void Save(SeasonalTaxFormTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalTaxFormTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal tax form templates. Empty fields match all.</summary>
public sealed record SeasonalTaxFormTemplateQuery(string Country, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalTaxFormTemplate item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalTaxFormTemplate> Apply(List<SeasonalTaxFormTemplate> items)
    {
        var matched = Collections.Filter(items, (SeasonalTaxFormTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal tax form template store.</summary>
public static class SeasonalTaxFormTemplateStore
{
    public static ISeasonalTaxFormTemplateStore NewMemorySeasonalTaxFormTemplateStore()
    {
        return new MemorySeasonalTaxFormTemplateStore(
            new Dictionary<string, SeasonalTaxFormTemplate>(),
            new List<string>());
    }

    public static ISeasonalTaxFormTemplateStore NewCachedSeasonalTaxFormTemplateStore(MemorySeasonalTaxFormTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalTaxFormTemplateStore(
            inner,
            SeasonalTaxFormTemplateCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalTaxFormTemplate> FindSeasonalTaxFormTemplates(ISeasonalTaxFormTemplateStore store, SeasonalTaxFormTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
