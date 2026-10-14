namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive warranty mappings by code.</summary>
public interface IAdaptiveWarrantyMappingStore
{
    AdaptiveWarrantyMapping? Find(string code);
    void Save(AdaptiveWarrantyMapping item);
    bool Remove(string code);
    List<AdaptiveWarrantyMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveWarrantyMappingStore(Dictionary<string, AdaptiveWarrantyMapping> items, List<string> order) : IAdaptiveWarrantyMappingStore
{
    public Dictionary<string, AdaptiveWarrantyMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveWarrantyMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveWarrantyMapping item)
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

    public List<AdaptiveWarrantyMapping> List()
    {
        var outValue = new List<AdaptiveWarrantyMapping>();
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
public sealed class CachedAdaptiveWarrantyMappingStore(MemoryAdaptiveWarrantyMappingStore inner, AdaptiveWarrantyMappingCache cache, int hits, int misses) : IAdaptiveWarrantyMappingStore
{
    public MemoryAdaptiveWarrantyMappingStore Inner { get; set; } = inner;
    public AdaptiveWarrantyMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveWarrantyMapping? Find(string code)
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

    public void Save(AdaptiveWarrantyMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveWarrantyMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive warranty mappings. Empty fields match all.</summary>
public sealed record AdaptiveWarrantyMappingQuery(string Label, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveWarrantyMapping item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveWarrantyMapping> Apply(List<AdaptiveWarrantyMapping> items)
    {
        var matched = Collections.Filter(items, (AdaptiveWarrantyMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive warranty mapping store.</summary>
public static class AdaptiveWarrantyMappingStore
{
    public static IAdaptiveWarrantyMappingStore NewMemoryAdaptiveWarrantyMappingStore()
    {
        return new MemoryAdaptiveWarrantyMappingStore(
            new Dictionary<string, AdaptiveWarrantyMapping>(),
            new List<string>());
    }

    public static IAdaptiveWarrantyMappingStore NewCachedAdaptiveWarrantyMappingStore(MemoryAdaptiveWarrantyMappingStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveWarrantyMappingStore(
            inner,
            AdaptiveWarrantyMappingCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveWarrantyMapping> FindAdaptiveWarrantyMappings(IAdaptiveWarrantyMappingStore store, AdaptiveWarrantyMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
