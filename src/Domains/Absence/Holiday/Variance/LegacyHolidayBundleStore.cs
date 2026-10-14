namespace Acme.Workforce.Absence.Holiday.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy holiday bundles by code.</summary>
public interface ILegacyHolidayBundleStore
{
    LegacyHolidayBundle? Find(string code);
    void Save(LegacyHolidayBundle item);
    bool Remove(string code);
    List<LegacyHolidayBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyHolidayBundleStore(Dictionary<string, LegacyHolidayBundle> items, List<string> order) : ILegacyHolidayBundleStore
{
    public Dictionary<string, LegacyHolidayBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyHolidayBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyHolidayBundle item)
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

    public List<LegacyHolidayBundle> List()
    {
        var outValue = new List<LegacyHolidayBundle>();
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
public sealed class CachedLegacyHolidayBundleStore(MemoryLegacyHolidayBundleStore inner, LegacyHolidayBundleCache cache, int hits, int misses) : ILegacyHolidayBundleStore
{
    public MemoryLegacyHolidayBundleStore Inner { get; set; } = inner;
    public LegacyHolidayBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyHolidayBundle? Find(string code)
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

    public void Save(LegacyHolidayBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyHolidayBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy holiday bundles. Empty fields match all.</summary>
public sealed record LegacyHolidayBundleQuery(string Region, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyHolidayBundle item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyHolidayBundle> Apply(List<LegacyHolidayBundle> items)
    {
        var matched = Collections.Filter(items, (LegacyHolidayBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy holiday bundle store.</summary>
public static class LegacyHolidayBundleStore
{
    public static ILegacyHolidayBundleStore NewMemoryLegacyHolidayBundleStore()
    {
        return new MemoryLegacyHolidayBundleStore(new Dictionary<string, LegacyHolidayBundle>(), new List<string>());
    }

    public static ILegacyHolidayBundleStore NewCachedLegacyHolidayBundleStore(MemoryLegacyHolidayBundleStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyHolidayBundleStore(inner, LegacyHolidayBundleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LegacyHolidayBundle> FindLegacyHolidayBundles(ILegacyHolidayBundleStore store, LegacyHolidayBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
