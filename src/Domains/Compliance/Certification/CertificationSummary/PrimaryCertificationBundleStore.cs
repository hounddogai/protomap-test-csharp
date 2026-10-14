namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary certification bundles by code.</summary>
public interface IPrimaryCertificationBundleStore
{
    PrimaryCertificationBundle? Find(string code);
    void Save(PrimaryCertificationBundle item);
    bool Remove(string code);
    List<PrimaryCertificationBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryCertificationBundleStore(Dictionary<string, PrimaryCertificationBundle> items, List<string> order) : IPrimaryCertificationBundleStore
{
    public Dictionary<string, PrimaryCertificationBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryCertificationBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryCertificationBundle item)
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

    public List<PrimaryCertificationBundle> List()
    {
        var outValue = new List<PrimaryCertificationBundle>();
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
public sealed class CachedPrimaryCertificationBundleStore(MemoryPrimaryCertificationBundleStore inner, PrimaryCertificationBundleCache cache, int hits, int misses) : IPrimaryCertificationBundleStore
{
    public MemoryPrimaryCertificationBundleStore Inner { get; set; } = inner;
    public PrimaryCertificationBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryCertificationBundle? Find(string code)
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

    public void Save(PrimaryCertificationBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryCertificationBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary certification bundles. Empty fields match all.</summary>
public sealed record PrimaryCertificationBundleQuery(string Country, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryCertificationBundle item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryCertificationBundle> Apply(List<PrimaryCertificationBundle> items)
    {
        var matched = Collections.Filter(items, (PrimaryCertificationBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary certification bundle store.</summary>
public static class PrimaryCertificationBundleStore
{
    public static IPrimaryCertificationBundleStore NewMemoryPrimaryCertificationBundleStore()
    {
        return new MemoryPrimaryCertificationBundleStore(
            new Dictionary<string, PrimaryCertificationBundle>(),
            new List<string>());
    }

    public static IPrimaryCertificationBundleStore NewCachedPrimaryCertificationBundleStore(MemoryPrimaryCertificationBundleStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryCertificationBundleStore(
            inner,
            PrimaryCertificationBundleCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryCertificationBundle> FindPrimaryCertificationBundles(IPrimaryCertificationBundleStore store, PrimaryCertificationBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
