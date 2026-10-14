namespace Acme.Workforce.Payrollops.TaxForm.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual tax form rollups by code.</summary>
public interface IManualTaxFormRollupStore
{
    ManualTaxFormRollup? Find(string code);
    void Save(ManualTaxFormRollup item);
    bool Remove(string code);
    List<ManualTaxFormRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualTaxFormRollupStore(Dictionary<string, ManualTaxFormRollup> items, List<string> order) : IManualTaxFormRollupStore
{
    public Dictionary<string, ManualTaxFormRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualTaxFormRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualTaxFormRollup item)
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

    public List<ManualTaxFormRollup> List()
    {
        var outValue = new List<ManualTaxFormRollup>();
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
public sealed class CachedManualTaxFormRollupStore(MemoryManualTaxFormRollupStore inner, ManualTaxFormRollupCache cache, int hits, int misses) : IManualTaxFormRollupStore
{
    public MemoryManualTaxFormRollupStore Inner { get; set; } = inner;
    public ManualTaxFormRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualTaxFormRollup? Find(string code)
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

    public void Save(ManualTaxFormRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualTaxFormRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual tax form rollups. Empty fields match all.</summary>
public sealed record ManualTaxFormRollupQuery(string Name, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(ManualTaxFormRollup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualTaxFormRollup> Apply(List<ManualTaxFormRollup> items)
    {
        var matched = Collections.Filter(items, (ManualTaxFormRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual tax form rollup store.</summary>
public static class ManualTaxFormRollupStore
{
    public static IManualTaxFormRollupStore NewMemoryManualTaxFormRollupStore()
    {
        return new MemoryManualTaxFormRollupStore(new Dictionary<string, ManualTaxFormRollup>(), new List<string>());
    }

    public static IManualTaxFormRollupStore NewCachedManualTaxFormRollupStore(MemoryManualTaxFormRollupStore inner, int ttl, IClock clock)
    {
        return new CachedManualTaxFormRollupStore(inner, ManualTaxFormRollupCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ManualTaxFormRollup> FindManualTaxFormRollups(IManualTaxFormRollupStore store, ManualTaxFormRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
