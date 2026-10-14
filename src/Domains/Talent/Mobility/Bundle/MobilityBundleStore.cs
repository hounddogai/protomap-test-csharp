namespace Acme.Workforce.Talent.Mobility.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility bundles by code.</summary>
public interface IMobilityBundleStore
{
    MobilityBundle? Find(string code);
    void Save(MobilityBundle item);
    bool Remove(string code);
    List<MobilityBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilityBundleStore(Dictionary<string, MobilityBundle> items, List<string> order) : IMobilityBundleStore
{
    public Dictionary<string, MobilityBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilityBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilityBundle item)
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

    public List<MobilityBundle> List()
    {
        var outValue = new List<MobilityBundle>();
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
public sealed class CachedMobilityBundleStore(MemoryMobilityBundleStore inner, MobilityBundleCache cache, int hits, int misses) : IMobilityBundleStore
{
    public MemoryMobilityBundleStore Inner { get; set; } = inner;
    public MobilityBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilityBundle? Find(string code)
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

    public void Save(MobilityBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilityBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility bundles. Empty fields match all.</summary>
public sealed record MobilityBundleQuery(string Title, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(MobilityBundle item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MobilityBundle> Apply(List<MobilityBundle> items)
    {
        var matched = Collections.Filter(items, (MobilityBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility bundle store.</summary>
public static class MobilityBundleStore
{
    public static IMobilityBundleStore NewMemoryMobilityBundleStore()
    {
        return new MemoryMobilityBundleStore(new Dictionary<string, MobilityBundle>(), new List<string>());
    }

    public static IMobilityBundleStore NewCachedMobilityBundleStore(MemoryMobilityBundleStore inner, int ttl, IClock clock)
    {
        return new CachedMobilityBundleStore(inner, MobilityBundleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<MobilityBundle> FindMobilityBundles(IMobilityBundleStore store, MobilityBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
