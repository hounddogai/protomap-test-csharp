namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted license seat bundles by code.</summary>
public interface IWeightedLicenseSeatBundleStore
{
    WeightedLicenseSeatBundle? Find(string code);
    void Save(WeightedLicenseSeatBundle item);
    bool Remove(string code);
    List<WeightedLicenseSeatBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedLicenseSeatBundleStore(Dictionary<string, WeightedLicenseSeatBundle> items, List<string> order) : IWeightedLicenseSeatBundleStore
{
    public Dictionary<string, WeightedLicenseSeatBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedLicenseSeatBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedLicenseSeatBundle item)
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

    public List<WeightedLicenseSeatBundle> List()
    {
        var outValue = new List<WeightedLicenseSeatBundle>();
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
public sealed class CachedWeightedLicenseSeatBundleStore(MemoryWeightedLicenseSeatBundleStore inner, WeightedLicenseSeatBundleCache cache, int hits, int misses) : IWeightedLicenseSeatBundleStore
{
    public MemoryWeightedLicenseSeatBundleStore Inner { get; set; } = inner;
    public WeightedLicenseSeatBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedLicenseSeatBundle? Find(string code)
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

    public void Save(WeightedLicenseSeatBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedLicenseSeatBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted license seat bundles. Empty fields match all.</summary>
public sealed record WeightedLicenseSeatBundleQuery(string Unit, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedLicenseSeatBundle item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedLicenseSeatBundle> Apply(List<WeightedLicenseSeatBundle> items)
    {
        var matched = Collections.Filter(items, (WeightedLicenseSeatBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted license seat bundle store.</summary>
public static class WeightedLicenseSeatBundleStore
{
    public static IWeightedLicenseSeatBundleStore NewMemoryWeightedLicenseSeatBundleStore()
    {
        return new MemoryWeightedLicenseSeatBundleStore(
            new Dictionary<string, WeightedLicenseSeatBundle>(),
            new List<string>());
    }

    public static IWeightedLicenseSeatBundleStore NewCachedWeightedLicenseSeatBundleStore(MemoryWeightedLicenseSeatBundleStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedLicenseSeatBundleStore(
            inner,
            WeightedLicenseSeatBundleCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<WeightedLicenseSeatBundle> FindWeightedLicenseSeatBundles(IWeightedLicenseSeatBundleStore store, WeightedLicenseSeatBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
