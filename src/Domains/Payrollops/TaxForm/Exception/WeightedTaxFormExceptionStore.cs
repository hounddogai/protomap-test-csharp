namespace Acme.Workforce.Payrollops.TaxForm.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted tax form exceptions by code.</summary>
public interface IWeightedTaxFormExceptionStore
{
    WeightedTaxFormException? Find(string code);
    void Save(WeightedTaxFormException item);
    bool Remove(string code);
    List<WeightedTaxFormException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedTaxFormExceptionStore(Dictionary<string, WeightedTaxFormException> items, List<string> order) : IWeightedTaxFormExceptionStore
{
    public Dictionary<string, WeightedTaxFormException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedTaxFormException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedTaxFormException item)
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

    public List<WeightedTaxFormException> List()
    {
        var outValue = new List<WeightedTaxFormException>();
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
public sealed class CachedWeightedTaxFormExceptionStore(MemoryWeightedTaxFormExceptionStore inner, WeightedTaxFormExceptionCache cache, int hits, int misses) : IWeightedTaxFormExceptionStore
{
    public MemoryWeightedTaxFormExceptionStore Inner { get; set; } = inner;
    public WeightedTaxFormExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedTaxFormException? Find(string code)
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

    public void Save(WeightedTaxFormException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedTaxFormException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted tax form exceptions. Empty fields match all.</summary>
public sealed record WeightedTaxFormExceptionQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedTaxFormException item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedTaxFormException> Apply(List<WeightedTaxFormException> items)
    {
        var matched = Collections.Filter(items, (WeightedTaxFormException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted tax form exception store.</summary>
public static class WeightedTaxFormExceptionStore
{
    public static IWeightedTaxFormExceptionStore NewMemoryWeightedTaxFormExceptionStore()
    {
        return new MemoryWeightedTaxFormExceptionStore(
            new Dictionary<string, WeightedTaxFormException>(),
            new List<string>());
    }

    public static IWeightedTaxFormExceptionStore NewCachedWeightedTaxFormExceptionStore(MemoryWeightedTaxFormExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedTaxFormExceptionStore(
            inner,
            WeightedTaxFormExceptionCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<WeightedTaxFormException> FindWeightedTaxFormExceptions(IWeightedTaxFormExceptionStore store, WeightedTaxFormExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
