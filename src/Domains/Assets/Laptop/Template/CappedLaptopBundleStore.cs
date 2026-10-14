namespace Acme.Workforce.Assets.Laptop.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped laptop bundles by code.</summary>
public interface ICappedLaptopBundleStore
{
    CappedLaptopBundle? Find(string code);
    void Save(CappedLaptopBundle item);
    bool Remove(string code);
    List<CappedLaptopBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedLaptopBundleStore(Dictionary<string, CappedLaptopBundle> items, List<string> order) : ICappedLaptopBundleStore
{
    public Dictionary<string, CappedLaptopBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedLaptopBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedLaptopBundle item)
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

    public List<CappedLaptopBundle> List()
    {
        var outValue = new List<CappedLaptopBundle>();
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
public sealed class CachedCappedLaptopBundleStore(MemoryCappedLaptopBundleStore inner, CappedLaptopBundleCache cache, int hits, int misses) : ICappedLaptopBundleStore
{
    public MemoryCappedLaptopBundleStore Inner { get; set; } = inner;
    public CappedLaptopBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedLaptopBundle? Find(string code)
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

    public void Save(CappedLaptopBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedLaptopBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped laptop bundles. Empty fields match all.</summary>
public sealed record CappedLaptopBundleQuery(string Category, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(CappedLaptopBundle item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedLaptopBundle> Apply(List<CappedLaptopBundle> items)
    {
        var matched = Collections.Filter(items, (CappedLaptopBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped laptop bundle store.</summary>
public static class CappedLaptopBundleStore
{
    public static ICappedLaptopBundleStore NewMemoryCappedLaptopBundleStore()
    {
        return new MemoryCappedLaptopBundleStore(new Dictionary<string, CappedLaptopBundle>(), new List<string>());
    }

    public static ICappedLaptopBundleStore NewCachedCappedLaptopBundleStore(MemoryCappedLaptopBundleStore inner, int ttl, IClock clock)
    {
        return new CachedCappedLaptopBundleStore(inner, CappedLaptopBundleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<CappedLaptopBundle> FindCappedLaptopBundles(ICappedLaptopBundleStore store, CappedLaptopBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
