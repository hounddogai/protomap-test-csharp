namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default access badge bundles by code.</summary>
public interface IDefaultAccessBadgeBundleStore
{
    DefaultAccessBadgeBundle? Find(string code);
    void Save(DefaultAccessBadgeBundle item);
    bool Remove(string code);
    List<DefaultAccessBadgeBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultAccessBadgeBundleStore(Dictionary<string, DefaultAccessBadgeBundle> items, List<string> order) : IDefaultAccessBadgeBundleStore
{
    public Dictionary<string, DefaultAccessBadgeBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultAccessBadgeBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultAccessBadgeBundle item)
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

    public List<DefaultAccessBadgeBundle> List()
    {
        var outValue = new List<DefaultAccessBadgeBundle>();
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
public sealed class CachedDefaultAccessBadgeBundleStore(MemoryDefaultAccessBadgeBundleStore inner, DefaultAccessBadgeBundleCache cache, int hits, int misses) : IDefaultAccessBadgeBundleStore
{
    public MemoryDefaultAccessBadgeBundleStore Inner { get; set; } = inner;
    public DefaultAccessBadgeBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultAccessBadgeBundle? Find(string code)
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

    public void Save(DefaultAccessBadgeBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultAccessBadgeBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default access badge bundles. Empty fields match all.</summary>
public sealed record DefaultAccessBadgeBundleQuery(string Locale, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultAccessBadgeBundle item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<DefaultAccessBadgeBundle> Apply(List<DefaultAccessBadgeBundle> items)
    {
        var matched = Collections.Filter(items, (DefaultAccessBadgeBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default access badge bundle store.</summary>
public static class DefaultAccessBadgeBundleStore
{
    public static IDefaultAccessBadgeBundleStore NewMemoryDefaultAccessBadgeBundleStore()
    {
        return new MemoryDefaultAccessBadgeBundleStore(
            new Dictionary<string, DefaultAccessBadgeBundle>(),
            new List<string>());
    }

    public static IDefaultAccessBadgeBundleStore NewCachedDefaultAccessBadgeBundleStore(MemoryDefaultAccessBadgeBundleStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultAccessBadgeBundleStore(
            inner,
            DefaultAccessBadgeBundleCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<DefaultAccessBadgeBundle> FindDefaultAccessBadgeBundles(IDefaultAccessBadgeBundleStore store, DefaultAccessBadgeBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
