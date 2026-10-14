namespace Acme.Workforce.Payrollops.TaxForm.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form cycles by code.</summary>
public interface ITaxFormCycleStore
{
    TaxFormCycle? Find(string code);
    void Save(TaxFormCycle item);
    bool Remove(string code);
    List<TaxFormCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormCycleStore(Dictionary<string, TaxFormCycle> items, List<string> order) : ITaxFormCycleStore
{
    public Dictionary<string, TaxFormCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormCycle item)
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

    public List<TaxFormCycle> List()
    {
        var outValue = new List<TaxFormCycle>();
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
public sealed class CachedTaxFormCycleStore(MemoryTaxFormCycleStore inner, TaxFormCycleCache cache, int hits, int misses) : ITaxFormCycleStore
{
    public MemoryTaxFormCycleStore Inner { get; set; } = inner;
    public TaxFormCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormCycle? Find(string code)
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

    public void Save(TaxFormCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form cycles. Empty fields match all.</summary>
public sealed record TaxFormCycleQuery(string Name, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormCycle item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<TaxFormCycle> Apply(List<TaxFormCycle> items)
    {
        var matched = Collections.Filter(items, (TaxFormCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form cycle store.</summary>
public static class TaxFormCycleStore
{
    public static ITaxFormCycleStore NewMemoryTaxFormCycleStore()
    {
        return new MemoryTaxFormCycleStore(new Dictionary<string, TaxFormCycle>(), new List<string>());
    }

    public static ITaxFormCycleStore NewCachedTaxFormCycleStore(MemoryTaxFormCycleStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormCycleStore(inner, TaxFormCycleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TaxFormCycle> FindTaxFormCycles(ITaxFormCycleStore store, TaxFormCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
