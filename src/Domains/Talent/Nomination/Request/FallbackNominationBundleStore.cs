namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback nomination bundles by code.</summary>
public interface IFallbackNominationBundleStore
{
    FallbackNominationBundle? Find(string code);
    void Save(FallbackNominationBundle item);
    bool Remove(string code);
    List<FallbackNominationBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackNominationBundleStore(Dictionary<string, FallbackNominationBundle> items, List<string> order) : IFallbackNominationBundleStore
{
    public Dictionary<string, FallbackNominationBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackNominationBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackNominationBundle item)
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

    public List<FallbackNominationBundle> List()
    {
        var outValue = new List<FallbackNominationBundle>();
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
public sealed class CachedFallbackNominationBundleStore(MemoryFallbackNominationBundleStore inner, FallbackNominationBundleCache cache, int hits, int misses) : IFallbackNominationBundleStore
{
    public MemoryFallbackNominationBundleStore Inner { get; set; } = inner;
    public FallbackNominationBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackNominationBundle? Find(string code)
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

    public void Save(FallbackNominationBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackNominationBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback nomination bundles. Empty fields match all.</summary>
public sealed record FallbackNominationBundleQuery(string Source, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackNominationBundle item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackNominationBundle> Apply(List<FallbackNominationBundle> items)
    {
        var matched = Collections.Filter(items, (FallbackNominationBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback nomination bundle store.</summary>
public static class FallbackNominationBundleStore
{
    public static IFallbackNominationBundleStore NewMemoryFallbackNominationBundleStore()
    {
        return new MemoryFallbackNominationBundleStore(
            new Dictionary<string, FallbackNominationBundle>(),
            new List<string>());
    }

    public static IFallbackNominationBundleStore NewCachedFallbackNominationBundleStore(MemoryFallbackNominationBundleStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackNominationBundleStore(
            inner,
            FallbackNominationBundleCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<FallbackNominationBundle> FindFallbackNominationBundles(IFallbackNominationBundleStore store, FallbackNominationBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
