namespace Acme.Workforce.Payrollops.TaxForm.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form entries by code.</summary>
public interface ITaxFormEntryStore
{
    TaxFormEntry? Find(string code);
    void Save(TaxFormEntry item);
    bool Remove(string code);
    List<TaxFormEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormEntryStore(Dictionary<string, TaxFormEntry> items, List<string> order) : ITaxFormEntryStore
{
    public Dictionary<string, TaxFormEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormEntry item)
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

    public List<TaxFormEntry> List()
    {
        var outValue = new List<TaxFormEntry>();
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
public sealed class CachedTaxFormEntryStore(MemoryTaxFormEntryStore inner, TaxFormEntryCache cache, int hits, int misses) : ITaxFormEntryStore
{
    public MemoryTaxFormEntryStore Inner { get; set; } = inner;
    public TaxFormEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormEntry? Find(string code)
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

    public void Save(TaxFormEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form entries. Empty fields match all.</summary>
public sealed record TaxFormEntryQuery(string Region, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormEntry item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<TaxFormEntry> Apply(List<TaxFormEntry> items)
    {
        var matched = Collections.Filter(items, (TaxFormEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form entry store.</summary>
public static class TaxFormEntryStore
{
    public static ITaxFormEntryStore NewMemoryTaxFormEntryStore()
    {
        return new MemoryTaxFormEntryStore(new Dictionary<string, TaxFormEntry>(), new List<string>());
    }

    public static ITaxFormEntryStore NewCachedTaxFormEntryStore(MemoryTaxFormEntryStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormEntryStore(inner, TaxFormEntryCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TaxFormEntry> FindTaxFormEntries(ITaxFormEntryStore store, TaxFormEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
