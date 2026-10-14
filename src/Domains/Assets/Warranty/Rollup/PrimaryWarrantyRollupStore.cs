namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary warranty rollups by code.</summary>
public interface IPrimaryWarrantyRollupStore
{
    PrimaryWarrantyRollup? Find(string code);
    void Save(PrimaryWarrantyRollup item);
    bool Remove(string code);
    List<PrimaryWarrantyRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryWarrantyRollupStore(Dictionary<string, PrimaryWarrantyRollup> items, List<string> order) : IPrimaryWarrantyRollupStore
{
    public Dictionary<string, PrimaryWarrantyRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryWarrantyRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryWarrantyRollup item)
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

    public List<PrimaryWarrantyRollup> List()
    {
        var outValue = new List<PrimaryWarrantyRollup>();
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
public sealed class CachedPrimaryWarrantyRollupStore(MemoryPrimaryWarrantyRollupStore inner, PrimaryWarrantyRollupCache cache, int hits, int misses) : IPrimaryWarrantyRollupStore
{
    public MemoryPrimaryWarrantyRollupStore Inner { get; set; } = inner;
    public PrimaryWarrantyRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryWarrantyRollup? Find(string code)
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

    public void Save(PrimaryWarrantyRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryWarrantyRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary warranty rollups. Empty fields match all.</summary>
public sealed record PrimaryWarrantyRollupQuery(string Title, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryWarrantyRollup item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryWarrantyRollup> Apply(List<PrimaryWarrantyRollup> items)
    {
        var matched = Collections.Filter(items, (PrimaryWarrantyRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary warranty rollup store.</summary>
public static class PrimaryWarrantyRollupStore
{
    public static IPrimaryWarrantyRollupStore NewMemoryPrimaryWarrantyRollupStore()
    {
        return new MemoryPrimaryWarrantyRollupStore(
            new Dictionary<string, PrimaryWarrantyRollup>(),
            new List<string>());
    }

    public static IPrimaryWarrantyRollupStore NewCachedPrimaryWarrantyRollupStore(MemoryPrimaryWarrantyRollupStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryWarrantyRollupStore(
            inner,
            PrimaryWarrantyRollupCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryWarrantyRollup> FindPrimaryWarrantyRollups(IPrimaryWarrantyRollupStore store, PrimaryWarrantyRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
