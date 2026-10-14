namespace Acme.Workforce.Payrollops.TaxForm.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual tax form cycles by code.</summary>
public interface IManualTaxFormCycleStore
{
    ManualTaxFormCycle? Find(string code);
    void Save(ManualTaxFormCycle item);
    bool Remove(string code);
    List<ManualTaxFormCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualTaxFormCycleStore(Dictionary<string, ManualTaxFormCycle> items, List<string> order) : IManualTaxFormCycleStore
{
    public Dictionary<string, ManualTaxFormCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualTaxFormCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualTaxFormCycle item)
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

    public List<ManualTaxFormCycle> List()
    {
        var outValue = new List<ManualTaxFormCycle>();
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
public sealed class CachedManualTaxFormCycleStore(MemoryManualTaxFormCycleStore inner, ManualTaxFormCycleCache cache, int hits, int misses) : IManualTaxFormCycleStore
{
    public MemoryManualTaxFormCycleStore Inner { get; set; } = inner;
    public ManualTaxFormCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualTaxFormCycle? Find(string code)
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

    public void Save(ManualTaxFormCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualTaxFormCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual tax form cycles. Empty fields match all.</summary>
public sealed record ManualTaxFormCycleQuery(string Locale, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(ManualTaxFormCycle item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualTaxFormCycle> Apply(List<ManualTaxFormCycle> items)
    {
        var matched = Collections.Filter(items, (ManualTaxFormCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual tax form cycle store.</summary>
public static class ManualTaxFormCycleStore
{
    public static IManualTaxFormCycleStore NewMemoryManualTaxFormCycleStore()
    {
        return new MemoryManualTaxFormCycleStore(new Dictionary<string, ManualTaxFormCycle>(), new List<string>());
    }

    public static IManualTaxFormCycleStore NewCachedManualTaxFormCycleStore(MemoryManualTaxFormCycleStore inner, int ttl, IClock clock)
    {
        return new CachedManualTaxFormCycleStore(inner, ManualTaxFormCycleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ManualTaxFormCycle> FindManualTaxFormCycles(IManualTaxFormCycleStore store, ManualTaxFormCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
