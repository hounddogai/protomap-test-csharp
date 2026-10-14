namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted access badge exceptions by code.</summary>
public interface IWeightedAccessBadgeExceptionStore
{
    WeightedAccessBadgeException? Find(string code);
    void Save(WeightedAccessBadgeException item);
    bool Remove(string code);
    List<WeightedAccessBadgeException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedAccessBadgeExceptionStore(Dictionary<string, WeightedAccessBadgeException> items, List<string> order) : IWeightedAccessBadgeExceptionStore
{
    public Dictionary<string, WeightedAccessBadgeException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedAccessBadgeException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedAccessBadgeException item)
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

    public List<WeightedAccessBadgeException> List()
    {
        var outValue = new List<WeightedAccessBadgeException>();
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
public sealed class CachedWeightedAccessBadgeExceptionStore(MemoryWeightedAccessBadgeExceptionStore inner, WeightedAccessBadgeExceptionCache cache, int hits, int misses) : IWeightedAccessBadgeExceptionStore
{
    public MemoryWeightedAccessBadgeExceptionStore Inner { get; set; } = inner;
    public WeightedAccessBadgeExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedAccessBadgeException? Find(string code)
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

    public void Save(WeightedAccessBadgeException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedAccessBadgeException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted access badge exceptions. Empty fields match all.</summary>
public sealed record WeightedAccessBadgeExceptionQuery(string Category, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedAccessBadgeException item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedAccessBadgeException> Apply(List<WeightedAccessBadgeException> items)
    {
        var matched = Collections.Filter(items, (WeightedAccessBadgeException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted access badge exception store.</summary>
public static class WeightedAccessBadgeExceptionStore
{
    public static IWeightedAccessBadgeExceptionStore NewMemoryWeightedAccessBadgeExceptionStore()
    {
        return new MemoryWeightedAccessBadgeExceptionStore(
            new Dictionary<string, WeightedAccessBadgeException>(),
            new List<string>());
    }

    public static IWeightedAccessBadgeExceptionStore NewCachedWeightedAccessBadgeExceptionStore(MemoryWeightedAccessBadgeExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedAccessBadgeExceptionStore(
            inner,
            WeightedAccessBadgeExceptionCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<WeightedAccessBadgeException> FindWeightedAccessBadgeExceptions(IWeightedAccessBadgeExceptionStore store, WeightedAccessBadgeExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
