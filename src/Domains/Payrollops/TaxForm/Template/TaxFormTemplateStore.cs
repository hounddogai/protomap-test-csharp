namespace Acme.Workforce.Payrollops.TaxForm.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form templates by code.</summary>
public interface ITaxFormTemplateStore
{
    TaxFormTemplate? Find(string code);
    void Save(TaxFormTemplate item);
    bool Remove(string code);
    List<TaxFormTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormTemplateStore(Dictionary<string, TaxFormTemplate> items, List<string> order) : ITaxFormTemplateStore
{
    public Dictionary<string, TaxFormTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormTemplate item)
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

    public List<TaxFormTemplate> List()
    {
        var outValue = new List<TaxFormTemplate>();
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
public sealed class CachedTaxFormTemplateStore(MemoryTaxFormTemplateStore inner, TaxFormTemplateCache cache, int hits, int misses) : ITaxFormTemplateStore
{
    public MemoryTaxFormTemplateStore Inner { get; set; } = inner;
    public TaxFormTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormTemplate? Find(string code)
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

    public void Save(TaxFormTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form templates. Empty fields match all.</summary>
public sealed record TaxFormTemplateQuery(string Label, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormTemplate item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TaxFormTemplate> Apply(List<TaxFormTemplate> items)
    {
        var matched = Collections.Filter(items, (TaxFormTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form template store.</summary>
public static class TaxFormTemplateStore
{
    public static ITaxFormTemplateStore NewMemoryTaxFormTemplateStore()
    {
        return new MemoryTaxFormTemplateStore(new Dictionary<string, TaxFormTemplate>(), new List<string>());
    }

    public static ITaxFormTemplateStore NewCachedTaxFormTemplateStore(MemoryTaxFormTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormTemplateStore(inner, TaxFormTemplateCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TaxFormTemplate> FindTaxFormTemplates(ITaxFormTemplateStore store, TaxFormTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
