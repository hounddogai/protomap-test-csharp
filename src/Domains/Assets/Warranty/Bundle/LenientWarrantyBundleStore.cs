namespace Acme.Workforce.Assets.Warranty.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient warranty bundles by code.</summary>
public interface ILenientWarrantyBundleStore
{
    LenientWarrantyBundle? Find(string code);
    void Save(LenientWarrantyBundle item);
    bool Remove(string code);
    List<LenientWarrantyBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientWarrantyBundleStore(Dictionary<string, LenientWarrantyBundle> items, List<string> order) : ILenientWarrantyBundleStore
{
    public Dictionary<string, LenientWarrantyBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientWarrantyBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientWarrantyBundle item)
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

    public List<LenientWarrantyBundle> List()
    {
        var outValue = new List<LenientWarrantyBundle>();
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
public sealed class CachedLenientWarrantyBundleStore(MemoryLenientWarrantyBundleStore inner, LenientWarrantyBundleCache cache, int hits, int misses) : ILenientWarrantyBundleStore
{
    public MemoryLenientWarrantyBundleStore Inner { get; set; } = inner;
    public LenientWarrantyBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientWarrantyBundle? Find(string code)
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

    public void Save(LenientWarrantyBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientWarrantyBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient warranty bundles. Empty fields match all.</summary>
public sealed record LenientWarrantyBundleQuery(string Label, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(LenientWarrantyBundle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<LenientWarrantyBundle> Apply(List<LenientWarrantyBundle> items)
    {
        var matched = Collections.Filter(items, (LenientWarrantyBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient warranty bundle store.</summary>
public static class LenientWarrantyBundleStore
{
    public static ILenientWarrantyBundleStore NewMemoryLenientWarrantyBundleStore()
    {
        return new MemoryLenientWarrantyBundleStore(
            new Dictionary<string, LenientWarrantyBundle>(),
            new List<string>());
    }

    public static ILenientWarrantyBundleStore NewCachedLenientWarrantyBundleStore(MemoryLenientWarrantyBundleStore inner, int ttl, IClock clock)
    {
        return new CachedLenientWarrantyBundleStore(inner, LenientWarrantyBundleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LenientWarrantyBundle> FindLenientWarrantyBundles(ILenientWarrantyBundleStore store, LenientWarrantyBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
