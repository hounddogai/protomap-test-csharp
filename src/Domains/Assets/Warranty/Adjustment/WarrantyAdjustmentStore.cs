namespace Acme.Workforce.Assets.Warranty.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty adjustments by code.</summary>
public interface IWarrantyAdjustmentStore
{
    WarrantyAdjustment? Find(string code);
    void Save(WarrantyAdjustment item);
    bool Remove(string code);
    List<WarrantyAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyAdjustmentStore(Dictionary<string, WarrantyAdjustment> items, List<string> order) : IWarrantyAdjustmentStore
{
    public Dictionary<string, WarrantyAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyAdjustment item)
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

    public List<WarrantyAdjustment> List()
    {
        var outValue = new List<WarrantyAdjustment>();
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
public sealed class CachedWarrantyAdjustmentStore(MemoryWarrantyAdjustmentStore inner, WarrantyAdjustmentCache cache, int hits, int misses) : IWarrantyAdjustmentStore
{
    public MemoryWarrantyAdjustmentStore Inner { get; set; } = inner;
    public WarrantyAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyAdjustment? Find(string code)
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

    public void Save(WarrantyAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty adjustments. Empty fields match all.</summary>
public sealed record WarrantyAdjustmentQuery(string Locale, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyAdjustment item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyAdjustment> Apply(List<WarrantyAdjustment> items)
    {
        var matched = Collections.Filter(items, (WarrantyAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty adjustment store.</summary>
public static class WarrantyAdjustmentStore
{
    public static IWarrantyAdjustmentStore NewMemoryWarrantyAdjustmentStore()
    {
        return new MemoryWarrantyAdjustmentStore(new Dictionary<string, WarrantyAdjustment>(), new List<string>());
    }

    public static IWarrantyAdjustmentStore NewCachedWarrantyAdjustmentStore(MemoryWarrantyAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyAdjustmentStore(inner, WarrantyAdjustmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<WarrantyAdjustment> FindWarrantyAdjustments(IWarrantyAdjustmentStore store, WarrantyAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
