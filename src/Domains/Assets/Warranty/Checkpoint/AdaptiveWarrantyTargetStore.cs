namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive warranty targets by code.</summary>
public interface IAdaptiveWarrantyTargetStore
{
    AdaptiveWarrantyTarget? Find(string code);
    void Save(AdaptiveWarrantyTarget item);
    bool Remove(string code);
    List<AdaptiveWarrantyTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveWarrantyTargetStore(Dictionary<string, AdaptiveWarrantyTarget> items, List<string> order) : IAdaptiveWarrantyTargetStore
{
    public Dictionary<string, AdaptiveWarrantyTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveWarrantyTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveWarrantyTarget item)
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

    public List<AdaptiveWarrantyTarget> List()
    {
        var outValue = new List<AdaptiveWarrantyTarget>();
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
public sealed class CachedAdaptiveWarrantyTargetStore(MemoryAdaptiveWarrantyTargetStore inner, AdaptiveWarrantyTargetCache cache, int hits, int misses) : IAdaptiveWarrantyTargetStore
{
    public MemoryAdaptiveWarrantyTargetStore Inner { get; set; } = inner;
    public AdaptiveWarrantyTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveWarrantyTarget? Find(string code)
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

    public void Save(AdaptiveWarrantyTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveWarrantyTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive warranty targets. Empty fields match all.</summary>
public sealed record AdaptiveWarrantyTargetQuery(string Locale, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveWarrantyTarget item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveWarrantyTarget> Apply(List<AdaptiveWarrantyTarget> items)
    {
        var matched = Collections.Filter(items, (AdaptiveWarrantyTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive warranty target store.</summary>
public static class AdaptiveWarrantyTargetStore
{
    public static IAdaptiveWarrantyTargetStore NewMemoryAdaptiveWarrantyTargetStore()
    {
        return new MemoryAdaptiveWarrantyTargetStore(
            new Dictionary<string, AdaptiveWarrantyTarget>(),
            new List<string>());
    }

    public static IAdaptiveWarrantyTargetStore NewCachedAdaptiveWarrantyTargetStore(MemoryAdaptiveWarrantyTargetStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveWarrantyTargetStore(
            inner,
            AdaptiveWarrantyTargetCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveWarrantyTarget> FindAdaptiveWarrantyTargets(IAdaptiveWarrantyTargetStore store, AdaptiveWarrantyTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
