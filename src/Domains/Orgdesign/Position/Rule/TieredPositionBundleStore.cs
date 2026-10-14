namespace Acme.Workforce.Orgdesign.Position.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered position bundles by code.</summary>
public interface ITieredPositionBundleStore
{
    TieredPositionBundle? Find(string code);
    void Save(TieredPositionBundle item);
    bool Remove(string code);
    List<TieredPositionBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredPositionBundleStore(Dictionary<string, TieredPositionBundle> items, List<string> order) : ITieredPositionBundleStore
{
    public Dictionary<string, TieredPositionBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredPositionBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredPositionBundle item)
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

    public List<TieredPositionBundle> List()
    {
        var outValue = new List<TieredPositionBundle>();
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
public sealed class CachedTieredPositionBundleStore(MemoryTieredPositionBundleStore inner, TieredPositionBundleCache cache, int hits, int misses) : ITieredPositionBundleStore
{
    public MemoryTieredPositionBundleStore Inner { get; set; } = inner;
    public TieredPositionBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredPositionBundle? Find(string code)
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

    public void Save(TieredPositionBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredPositionBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered position bundles. Empty fields match all.</summary>
public sealed record TieredPositionBundleQuery(string Segment, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(TieredPositionBundle item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<TieredPositionBundle> Apply(List<TieredPositionBundle> items)
    {
        var matched = Collections.Filter(items, (TieredPositionBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered position bundle store.</summary>
public static class TieredPositionBundleStore
{
    public static ITieredPositionBundleStore NewMemoryTieredPositionBundleStore()
    {
        return new MemoryTieredPositionBundleStore(
            new Dictionary<string, TieredPositionBundle>(),
            new List<string>());
    }

    public static ITieredPositionBundleStore NewCachedTieredPositionBundleStore(MemoryTieredPositionBundleStore inner, int ttl, IClock clock)
    {
        return new CachedTieredPositionBundleStore(inner, TieredPositionBundleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TieredPositionBundle> FindTieredPositionBundles(ITieredPositionBundleStore store, TieredPositionBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
