namespace Acme.Workforce.Payrollops.TaxForm.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form targets by code.</summary>
public interface ITaxFormTargetStore
{
    TaxFormTarget? Find(string code);
    void Save(TaxFormTarget item);
    bool Remove(string code);
    List<TaxFormTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormTargetStore(Dictionary<string, TaxFormTarget> items, List<string> order) : ITaxFormTargetStore
{
    public Dictionary<string, TaxFormTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormTarget item)
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

    public List<TaxFormTarget> List()
    {
        var outValue = new List<TaxFormTarget>();
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
public sealed class CachedTaxFormTargetStore(MemoryTaxFormTargetStore inner, TaxFormTargetCache cache, int hits, int misses) : ITaxFormTargetStore
{
    public MemoryTaxFormTargetStore Inner { get; set; } = inner;
    public TaxFormTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormTarget? Find(string code)
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

    public void Save(TaxFormTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form targets. Empty fields match all.</summary>
public sealed record TaxFormTargetQuery(string Title, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormTarget item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TaxFormTarget> Apply(List<TaxFormTarget> items)
    {
        var matched = Collections.Filter(items, (TaxFormTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form target store.</summary>
public static class TaxFormTargetStore
{
    public static ITaxFormTargetStore NewMemoryTaxFormTargetStore()
    {
        return new MemoryTaxFormTargetStore(new Dictionary<string, TaxFormTarget>(), new List<string>());
    }

    public static ITaxFormTargetStore NewCachedTaxFormTargetStore(MemoryTaxFormTargetStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormTargetStore(inner, TaxFormTargetCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TaxFormTarget> FindTaxFormTargets(ITaxFormTargetStore store, TaxFormTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
