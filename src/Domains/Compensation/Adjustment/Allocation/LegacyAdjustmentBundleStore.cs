namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy adjustment bundles by code.</summary>
public interface ILegacyAdjustmentBundleStore
{
    LegacyAdjustmentBundle? Find(string code);
    void Save(LegacyAdjustmentBundle item);
    bool Remove(string code);
    List<LegacyAdjustmentBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyAdjustmentBundleStore(Dictionary<string, LegacyAdjustmentBundle> items, List<string> order) : ILegacyAdjustmentBundleStore
{
    public Dictionary<string, LegacyAdjustmentBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyAdjustmentBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyAdjustmentBundle item)
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

    public List<LegacyAdjustmentBundle> List()
    {
        var outValue = new List<LegacyAdjustmentBundle>();
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
public sealed class CachedLegacyAdjustmentBundleStore(MemoryLegacyAdjustmentBundleStore inner, LegacyAdjustmentBundleCache cache, int hits, int misses) : ILegacyAdjustmentBundleStore
{
    public MemoryLegacyAdjustmentBundleStore Inner { get; set; } = inner;
    public LegacyAdjustmentBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyAdjustmentBundle? Find(string code)
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

    public void Save(LegacyAdjustmentBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyAdjustmentBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy adjustment bundles. Empty fields match all.</summary>
public sealed record LegacyAdjustmentBundleQuery(string Name, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyAdjustmentBundle item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyAdjustmentBundle> Apply(List<LegacyAdjustmentBundle> items)
    {
        var matched = Collections.Filter(items, (LegacyAdjustmentBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy adjustment bundle store.</summary>
public static class LegacyAdjustmentBundleStore
{
    public static ILegacyAdjustmentBundleStore NewMemoryLegacyAdjustmentBundleStore()
    {
        return new MemoryLegacyAdjustmentBundleStore(
            new Dictionary<string, LegacyAdjustmentBundle>(),
            new List<string>());
    }

    public static ILegacyAdjustmentBundleStore NewCachedLegacyAdjustmentBundleStore(MemoryLegacyAdjustmentBundleStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyAdjustmentBundleStore(
            inner,
            LegacyAdjustmentBundleCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LegacyAdjustmentBundle> FindLegacyAdjustmentBundles(ILegacyAdjustmentBundleStore store, LegacyAdjustmentBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
