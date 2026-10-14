namespace Acme.Workforce.Assets.Laptop.LaptopManifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual laptop bundles by code.</summary>
public interface IManualLaptopBundleStore
{
    ManualLaptopBundle? Find(string code);
    void Save(ManualLaptopBundle item);
    bool Remove(string code);
    List<ManualLaptopBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualLaptopBundleStore(Dictionary<string, ManualLaptopBundle> items, List<string> order) : IManualLaptopBundleStore
{
    public Dictionary<string, ManualLaptopBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualLaptopBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualLaptopBundle item)
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

    public List<ManualLaptopBundle> List()
    {
        var outValue = new List<ManualLaptopBundle>();
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
public sealed class CachedManualLaptopBundleStore(MemoryManualLaptopBundleStore inner, ManualLaptopBundleCache cache, int hits, int misses) : IManualLaptopBundleStore
{
    public MemoryManualLaptopBundleStore Inner { get; set; } = inner;
    public ManualLaptopBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualLaptopBundle? Find(string code)
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

    public void Save(ManualLaptopBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualLaptopBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual laptop bundles. Empty fields match all.</summary>
public sealed record ManualLaptopBundleQuery(string Segment, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(ManualLaptopBundle item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualLaptopBundle> Apply(List<ManualLaptopBundle> items)
    {
        var matched = Collections.Filter(items, (ManualLaptopBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual laptop bundle store.</summary>
public static class ManualLaptopBundleStore
{
    public static IManualLaptopBundleStore NewMemoryManualLaptopBundleStore()
    {
        return new MemoryManualLaptopBundleStore(new Dictionary<string, ManualLaptopBundle>(), new List<string>());
    }

    public static IManualLaptopBundleStore NewCachedManualLaptopBundleStore(MemoryManualLaptopBundleStore inner, int ttl, IClock clock)
    {
        return new CachedManualLaptopBundleStore(inner, ManualLaptopBundleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ManualLaptopBundle> FindManualLaptopBundles(IManualLaptopBundleStore store, ManualLaptopBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
