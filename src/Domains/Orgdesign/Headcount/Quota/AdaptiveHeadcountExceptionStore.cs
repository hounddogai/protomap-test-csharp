namespace Acme.Workforce.Orgdesign.Headcount.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive headcount exceptions by code.</summary>
public interface IAdaptiveHeadcountExceptionStore
{
    AdaptiveHeadcountException? Find(string code);
    void Save(AdaptiveHeadcountException item);
    bool Remove(string code);
    List<AdaptiveHeadcountException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveHeadcountExceptionStore(Dictionary<string, AdaptiveHeadcountException> items, List<string> order) : IAdaptiveHeadcountExceptionStore
{
    public Dictionary<string, AdaptiveHeadcountException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveHeadcountException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveHeadcountException item)
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

    public List<AdaptiveHeadcountException> List()
    {
        var outValue = new List<AdaptiveHeadcountException>();
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
public sealed class CachedAdaptiveHeadcountExceptionStore(MemoryAdaptiveHeadcountExceptionStore inner, AdaptiveHeadcountExceptionCache cache, int hits, int misses) : IAdaptiveHeadcountExceptionStore
{
    public MemoryAdaptiveHeadcountExceptionStore Inner { get; set; } = inner;
    public AdaptiveHeadcountExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveHeadcountException? Find(string code)
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

    public void Save(AdaptiveHeadcountException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveHeadcountException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive headcount exceptions. Empty fields match all.</summary>
public sealed record AdaptiveHeadcountExceptionQuery(string Region, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveHeadcountException item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveHeadcountException> Apply(List<AdaptiveHeadcountException> items)
    {
        var matched = Collections.Filter(items, (AdaptiveHeadcountException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive headcount exception store.</summary>
public static class AdaptiveHeadcountExceptionStore
{
    public static IAdaptiveHeadcountExceptionStore NewMemoryAdaptiveHeadcountExceptionStore()
    {
        return new MemoryAdaptiveHeadcountExceptionStore(
            new Dictionary<string, AdaptiveHeadcountException>(),
            new List<string>());
    }

    public static IAdaptiveHeadcountExceptionStore NewCachedAdaptiveHeadcountExceptionStore(MemoryAdaptiveHeadcountExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveHeadcountExceptionStore(
            inner,
            AdaptiveHeadcountExceptionCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveHeadcountException> FindAdaptiveHeadcountExceptions(IAdaptiveHeadcountExceptionStore store, AdaptiveHeadcountExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
