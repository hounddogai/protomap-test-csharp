namespace Acme.Workforce.Talent.CareerPath.GlobalPathRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive career path exceptions by code.</summary>
public interface IAdaptiveCareerPathExceptionStore
{
    AdaptiveCareerPathException? Find(string code);
    void Save(AdaptiveCareerPathException item);
    bool Remove(string code);
    List<AdaptiveCareerPathException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveCareerPathExceptionStore(Dictionary<string, AdaptiveCareerPathException> items, List<string> order) : IAdaptiveCareerPathExceptionStore
{
    public Dictionary<string, AdaptiveCareerPathException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveCareerPathException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveCareerPathException item)
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

    public List<AdaptiveCareerPathException> List()
    {
        var outValue = new List<AdaptiveCareerPathException>();
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
public sealed class CachedAdaptiveCareerPathExceptionStore(MemoryAdaptiveCareerPathExceptionStore inner, AdaptiveCareerPathExceptionCache cache, int hits, int misses) : IAdaptiveCareerPathExceptionStore
{
    public MemoryAdaptiveCareerPathExceptionStore Inner { get; set; } = inner;
    public AdaptiveCareerPathExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveCareerPathException? Find(string code)
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

    public void Save(AdaptiveCareerPathException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveCareerPathException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive career path exceptions. Empty fields match all.</summary>
public sealed record AdaptiveCareerPathExceptionQuery(string Category, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveCareerPathException item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveCareerPathException> Apply(List<AdaptiveCareerPathException> items)
    {
        var matched = Collections.Filter(items, (AdaptiveCareerPathException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive career path exception store.</summary>
public static class AdaptiveCareerPathExceptionStore
{
    public static IAdaptiveCareerPathExceptionStore NewMemoryAdaptiveCareerPathExceptionStore()
    {
        return new MemoryAdaptiveCareerPathExceptionStore(
            new Dictionary<string, AdaptiveCareerPathException>(),
            new List<string>());
    }

    public static IAdaptiveCareerPathExceptionStore NewCachedAdaptiveCareerPathExceptionStore(MemoryAdaptiveCareerPathExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveCareerPathExceptionStore(
            inner,
            AdaptiveCareerPathExceptionCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveCareerPathException> FindAdaptiveCareerPathExceptions(IAdaptiveCareerPathExceptionStore store, AdaptiveCareerPathExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
