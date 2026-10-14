namespace Acme.Workforce.Assets.Laptop.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop adjustments by code.</summary>
public interface ILaptopAdjustmentStore
{
    LaptopAdjustment? Find(string code);
    void Save(LaptopAdjustment item);
    bool Remove(string code);
    List<LaptopAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopAdjustmentStore(Dictionary<string, LaptopAdjustment> items, List<string> order) : ILaptopAdjustmentStore
{
    public Dictionary<string, LaptopAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopAdjustment item)
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

    public List<LaptopAdjustment> List()
    {
        var outValue = new List<LaptopAdjustment>();
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
public sealed class CachedLaptopAdjustmentStore(MemoryLaptopAdjustmentStore inner, LaptopAdjustmentCache cache, int hits, int misses) : ILaptopAdjustmentStore
{
    public MemoryLaptopAdjustmentStore Inner { get; set; } = inner;
    public LaptopAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopAdjustment? Find(string code)
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

    public void Save(LaptopAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop adjustments. Empty fields match all.</summary>
public sealed record LaptopAdjustmentQuery(string Title, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopAdjustment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopAdjustment> Apply(List<LaptopAdjustment> items)
    {
        var matched = Collections.Filter(items, (LaptopAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop adjustment store.</summary>
public static class LaptopAdjustmentStore
{
    public static ILaptopAdjustmentStore NewMemoryLaptopAdjustmentStore()
    {
        return new MemoryLaptopAdjustmentStore(new Dictionary<string, LaptopAdjustment>(), new List<string>());
    }

    public static ILaptopAdjustmentStore NewCachedLaptopAdjustmentStore(MemoryLaptopAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopAdjustmentStore(inner, LaptopAdjustmentCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LaptopAdjustment> FindLaptopAdjustments(ILaptopAdjustmentStore store, LaptopAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
