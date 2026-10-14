namespace Acme.Workforce.Assets.Warranty.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty bundles by code.</summary>
public interface IWarrantyBundleStore
{
    WarrantyBundle? Find(string code);
    void Save(WarrantyBundle item);
    bool Remove(string code);
    List<WarrantyBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyBundleStore(Dictionary<string, WarrantyBundle> items, List<string> order) : IWarrantyBundleStore
{
    public Dictionary<string, WarrantyBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyBundle item)
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

    public List<WarrantyBundle> List()
    {
        var outValue = new List<WarrantyBundle>();
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
public sealed class CachedWarrantyBundleStore(MemoryWarrantyBundleStore inner, WarrantyBundleCache cache, int hits, int misses) : IWarrantyBundleStore
{
    public MemoryWarrantyBundleStore Inner { get; set; } = inner;
    public WarrantyBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyBundle? Find(string code)
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

    public void Save(WarrantyBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty bundles. Empty fields match all.</summary>
public sealed record WarrantyBundleQuery(string Unit, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyBundle item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyBundle> Apply(List<WarrantyBundle> items)
    {
        var matched = Collections.Filter(items, (WarrantyBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty bundle store.</summary>
public static class WarrantyBundleStore
{
    public static IWarrantyBundleStore NewMemoryWarrantyBundleStore()
    {
        return new MemoryWarrantyBundleStore(new Dictionary<string, WarrantyBundle>(), new List<string>());
    }

    public static IWarrantyBundleStore NewCachedWarrantyBundleStore(MemoryWarrantyBundleStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyBundleStore(inner, WarrantyBundleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<WarrantyBundle> FindWarrantyBundles(IWarrantyBundleStore store, WarrantyBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
