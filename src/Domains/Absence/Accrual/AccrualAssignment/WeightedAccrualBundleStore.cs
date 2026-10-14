namespace Acme.Workforce.Absence.Accrual.AccrualAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted accrual bundles by code.</summary>
public interface IWeightedAccrualBundleStore
{
    WeightedAccrualBundle? Find(string code);
    void Save(WeightedAccrualBundle item);
    bool Remove(string code);
    List<WeightedAccrualBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedAccrualBundleStore(Dictionary<string, WeightedAccrualBundle> items, List<string> order) : IWeightedAccrualBundleStore
{
    public Dictionary<string, WeightedAccrualBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedAccrualBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedAccrualBundle item)
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

    public List<WeightedAccrualBundle> List()
    {
        var outValue = new List<WeightedAccrualBundle>();
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
public sealed class CachedWeightedAccrualBundleStore(MemoryWeightedAccrualBundleStore inner, WeightedAccrualBundleCache cache, int hits, int misses) : IWeightedAccrualBundleStore
{
    public MemoryWeightedAccrualBundleStore Inner { get; set; } = inner;
    public WeightedAccrualBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedAccrualBundle? Find(string code)
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

    public void Save(WeightedAccrualBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedAccrualBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted accrual bundles. Empty fields match all.</summary>
public sealed record WeightedAccrualBundleQuery(string Name, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedAccrualBundle item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedAccrualBundle> Apply(List<WeightedAccrualBundle> items)
    {
        var matched = Collections.Filter(items, (WeightedAccrualBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted accrual bundle store.</summary>
public static class WeightedAccrualBundleStore
{
    public static IWeightedAccrualBundleStore NewMemoryWeightedAccrualBundleStore()
    {
        return new MemoryWeightedAccrualBundleStore(
            new Dictionary<string, WeightedAccrualBundle>(),
            new List<string>());
    }

    public static IWeightedAccrualBundleStore NewCachedWeightedAccrualBundleStore(MemoryWeightedAccrualBundleStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedAccrualBundleStore(inner, WeightedAccrualBundleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<WeightedAccrualBundle> FindWeightedAccrualBundles(IWeightedAccrualBundleStore store, WeightedAccrualBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
