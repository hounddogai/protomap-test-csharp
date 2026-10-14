namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form windows by code.</summary>
public interface ITaxFormWindowStore
{
    TaxFormWindow? Find(string code);
    void Save(TaxFormWindow item);
    bool Remove(string code);
    List<TaxFormWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormWindowStore(Dictionary<string, TaxFormWindow> items, List<string> order) : ITaxFormWindowStore
{
    public Dictionary<string, TaxFormWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormWindow item)
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

    public List<TaxFormWindow> List()
    {
        var outValue = new List<TaxFormWindow>();
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
public sealed class CachedTaxFormWindowStore(MemoryTaxFormWindowStore inner, TaxFormWindowCache cache, int hits, int misses) : ITaxFormWindowStore
{
    public MemoryTaxFormWindowStore Inner { get; set; } = inner;
    public TaxFormWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormWindow? Find(string code)
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

    public void Save(TaxFormWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form windows. Empty fields match all.</summary>
public sealed record TaxFormWindowQuery(string Name, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormWindow item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TaxFormWindow> Apply(List<TaxFormWindow> items)
    {
        var matched = Collections.Filter(items, (TaxFormWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form window store.</summary>
public static class TaxFormWindowStore
{
    public static ITaxFormWindowStore NewMemoryTaxFormWindowStore()
    {
        return new MemoryTaxFormWindowStore(new Dictionary<string, TaxFormWindow>(), new List<string>());
    }

    public static ITaxFormWindowStore NewCachedTaxFormWindowStore(MemoryTaxFormWindowStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormWindowStore(inner, TaxFormWindowCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TaxFormWindow> FindTaxFormWindows(ITaxFormWindowStore store, TaxFormWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
