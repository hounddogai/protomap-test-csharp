namespace Acme.Workforce.Orgdesign.Position.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position bundles by code.</summary>
public interface IPositionBundleStore
{
    PositionBundle? Find(string code);
    void Save(PositionBundle item);
    bool Remove(string code);
    List<PositionBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionBundleStore(Dictionary<string, PositionBundle> items, List<string> order) : IPositionBundleStore
{
    public Dictionary<string, PositionBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionBundle item)
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

    public List<PositionBundle> List()
    {
        var outValue = new List<PositionBundle>();
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
public sealed class CachedPositionBundleStore(MemoryPositionBundleStore inner, PositionBundleCache cache, int hits, int misses) : IPositionBundleStore
{
    public MemoryPositionBundleStore Inner { get; set; } = inner;
    public PositionBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionBundle? Find(string code)
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

    public void Save(PositionBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position bundles. Empty fields match all.</summary>
public sealed record PositionBundleQuery(string Department, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PositionBundle item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PositionBundle> Apply(List<PositionBundle> items)
    {
        var matched = Collections.Filter(items, (PositionBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position bundle store.</summary>
public static class PositionBundleStore
{
    public static IPositionBundleStore NewMemoryPositionBundleStore()
    {
        return new MemoryPositionBundleStore(new Dictionary<string, PositionBundle>(), new List<string>());
    }

    public static IPositionBundleStore NewCachedPositionBundleStore(MemoryPositionBundleStore inner, int ttl, IClock clock)
    {
        return new CachedPositionBundleStore(inner, PositionBundleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PositionBundle> FindPositionBundles(IPositionBundleStore store, PositionBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
