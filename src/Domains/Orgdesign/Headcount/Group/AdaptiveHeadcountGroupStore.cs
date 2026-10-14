namespace Acme.Workforce.Orgdesign.Headcount.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive headcount groups by code.</summary>
public interface IAdaptiveHeadcountGroupStore
{
    AdaptiveHeadcountGroup? Find(string code);
    void Save(AdaptiveHeadcountGroup item);
    bool Remove(string code);
    List<AdaptiveHeadcountGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveHeadcountGroupStore(Dictionary<string, AdaptiveHeadcountGroup> items, List<string> order) : IAdaptiveHeadcountGroupStore
{
    public Dictionary<string, AdaptiveHeadcountGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveHeadcountGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveHeadcountGroup item)
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

    public List<AdaptiveHeadcountGroup> List()
    {
        var outValue = new List<AdaptiveHeadcountGroup>();
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
public sealed class CachedAdaptiveHeadcountGroupStore(MemoryAdaptiveHeadcountGroupStore inner, AdaptiveHeadcountGroupCache cache, int hits, int misses) : IAdaptiveHeadcountGroupStore
{
    public MemoryAdaptiveHeadcountGroupStore Inner { get; set; } = inner;
    public AdaptiveHeadcountGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveHeadcountGroup? Find(string code)
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

    public void Save(AdaptiveHeadcountGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveHeadcountGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive headcount groups. Empty fields match all.</summary>
public sealed record AdaptiveHeadcountGroupQuery(string Locale, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveHeadcountGroup item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<AdaptiveHeadcountGroup> Apply(List<AdaptiveHeadcountGroup> items)
    {
        var matched = Collections.Filter(items, (AdaptiveHeadcountGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive headcount group store.</summary>
public static class AdaptiveHeadcountGroupStore
{
    public static IAdaptiveHeadcountGroupStore NewMemoryAdaptiveHeadcountGroupStore()
    {
        return new MemoryAdaptiveHeadcountGroupStore(
            new Dictionary<string, AdaptiveHeadcountGroup>(),
            new List<string>());
    }

    public static IAdaptiveHeadcountGroupStore NewCachedAdaptiveHeadcountGroupStore(MemoryAdaptiveHeadcountGroupStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveHeadcountGroupStore(
            inner,
            AdaptiveHeadcountGroupCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveHeadcountGroup> FindAdaptiveHeadcountGroups(IAdaptiveHeadcountGroupStore store, AdaptiveHeadcountGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
