namespace Acme.Workforce.Contractors.Offboarding.OffboardingBundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict offboarding bundles by code.</summary>
public interface IStrictOffboardingBundleStore
{
    StrictOffboardingBundle? Find(string code);
    void Save(StrictOffboardingBundle item);
    bool Remove(string code);
    List<StrictOffboardingBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictOffboardingBundleStore(Dictionary<string, StrictOffboardingBundle> items, List<string> order) : IStrictOffboardingBundleStore
{
    public Dictionary<string, StrictOffboardingBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictOffboardingBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictOffboardingBundle item)
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

    public List<StrictOffboardingBundle> List()
    {
        var outValue = new List<StrictOffboardingBundle>();
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
public sealed class CachedStrictOffboardingBundleStore(MemoryStrictOffboardingBundleStore inner, StrictOffboardingBundleCache cache, int hits, int misses) : IStrictOffboardingBundleStore
{
    public MemoryStrictOffboardingBundleStore Inner { get; set; } = inner;
    public StrictOffboardingBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictOffboardingBundle? Find(string code)
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

    public void Save(StrictOffboardingBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictOffboardingBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict offboarding bundles. Empty fields match all.</summary>
public sealed record StrictOffboardingBundleQuery(string Department, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(StrictOffboardingBundle item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<StrictOffboardingBundle> Apply(List<StrictOffboardingBundle> items)
    {
        var matched = Collections.Filter(items, (StrictOffboardingBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict offboarding bundle store.</summary>
public static class StrictOffboardingBundleStore
{
    public static IStrictOffboardingBundleStore NewMemoryStrictOffboardingBundleStore()
    {
        return new MemoryStrictOffboardingBundleStore(
            new Dictionary<string, StrictOffboardingBundle>(),
            new List<string>());
    }

    public static IStrictOffboardingBundleStore NewCachedStrictOffboardingBundleStore(MemoryStrictOffboardingBundleStore inner, int ttl, IClock clock)
    {
        return new CachedStrictOffboardingBundleStore(
            inner,
            StrictOffboardingBundleCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StrictOffboardingBundle> FindStrictOffboardingBundles(IStrictOffboardingBundleStore store, StrictOffboardingBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
