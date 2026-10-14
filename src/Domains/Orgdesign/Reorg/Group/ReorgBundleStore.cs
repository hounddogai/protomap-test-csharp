namespace Acme.Workforce.Orgdesign.Reorg.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg bundles by code.</summary>
public interface IReorgBundleStore
{
    ReorgBundle? Find(string code);
    void Save(ReorgBundle item);
    bool Remove(string code);
    List<ReorgBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgBundleStore(Dictionary<string, ReorgBundle> items, List<string> order) : IReorgBundleStore
{
    public Dictionary<string, ReorgBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgBundle item)
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

    public List<ReorgBundle> List()
    {
        var outValue = new List<ReorgBundle>();
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
public sealed class CachedReorgBundleStore(MemoryReorgBundleStore inner, ReorgBundleCache cache, int hits, int misses) : IReorgBundleStore
{
    public MemoryReorgBundleStore Inner { get; set; } = inner;
    public ReorgBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgBundle? Find(string code)
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

    public void Save(ReorgBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg bundles. Empty fields match all.</summary>
public sealed record ReorgBundleQuery(string Team, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgBundle item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReorgBundle> Apply(List<ReorgBundle> items)
    {
        var matched = Collections.Filter(items, (ReorgBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg bundle store.</summary>
public static class ReorgBundleStore
{
    public static IReorgBundleStore NewMemoryReorgBundleStore()
    {
        return new MemoryReorgBundleStore(new Dictionary<string, ReorgBundle>(), new List<string>());
    }

    public static IReorgBundleStore NewCachedReorgBundleStore(MemoryReorgBundleStore inner, int ttl, IClock clock)
    {
        return new CachedReorgBundleStore(inner, ReorgBundleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ReorgBundle> FindReorgBundles(IReorgBundleStore store, ReorgBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
