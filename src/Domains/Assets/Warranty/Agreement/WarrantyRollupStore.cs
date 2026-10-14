namespace Acme.Workforce.Assets.Warranty.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty rollups by code.</summary>
public interface IWarrantyRollupStore
{
    WarrantyRollup? Find(string code);
    void Save(WarrantyRollup item);
    bool Remove(string code);
    List<WarrantyRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyRollupStore(Dictionary<string, WarrantyRollup> items, List<string> order) : IWarrantyRollupStore
{
    public Dictionary<string, WarrantyRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyRollup item)
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

    public List<WarrantyRollup> List()
    {
        var outValue = new List<WarrantyRollup>();
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
public sealed class CachedWarrantyRollupStore(MemoryWarrantyRollupStore inner, WarrantyRollupCache cache, int hits, int misses) : IWarrantyRollupStore
{
    public MemoryWarrantyRollupStore Inner { get; set; } = inner;
    public WarrantyRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyRollup? Find(string code)
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

    public void Save(WarrantyRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty rollups. Empty fields match all.</summary>
public sealed record WarrantyRollupQuery(string Name, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyRollup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<WarrantyRollup> Apply(List<WarrantyRollup> items)
    {
        var matched = Collections.Filter(items, (WarrantyRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty rollup store.</summary>
public static class WarrantyRollupStore
{
    public static IWarrantyRollupStore NewMemoryWarrantyRollupStore()
    {
        return new MemoryWarrantyRollupStore(new Dictionary<string, WarrantyRollup>(), new List<string>());
    }

    public static IWarrantyRollupStore NewCachedWarrantyRollupStore(MemoryWarrantyRollupStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyRollupStore(inner, WarrantyRollupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<WarrantyRollup> FindWarrantyRollups(IWarrantyRollupStore store, WarrantyRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
