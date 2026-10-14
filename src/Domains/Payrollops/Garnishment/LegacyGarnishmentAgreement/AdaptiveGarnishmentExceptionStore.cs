namespace Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive garnishment exceptions by code.</summary>
public interface IAdaptiveGarnishmentExceptionStore
{
    AdaptiveGarnishmentException? Find(string code);
    void Save(AdaptiveGarnishmentException item);
    bool Remove(string code);
    List<AdaptiveGarnishmentException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveGarnishmentExceptionStore(Dictionary<string, AdaptiveGarnishmentException> items, List<string> order) : IAdaptiveGarnishmentExceptionStore
{
    public Dictionary<string, AdaptiveGarnishmentException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveGarnishmentException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveGarnishmentException item)
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

    public List<AdaptiveGarnishmentException> List()
    {
        var outValue = new List<AdaptiveGarnishmentException>();
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
public sealed class CachedAdaptiveGarnishmentExceptionStore(MemoryAdaptiveGarnishmentExceptionStore inner, AdaptiveGarnishmentExceptionCache cache, int hits, int misses) : IAdaptiveGarnishmentExceptionStore
{
    public MemoryAdaptiveGarnishmentExceptionStore Inner { get; set; } = inner;
    public AdaptiveGarnishmentExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveGarnishmentException? Find(string code)
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

    public void Save(AdaptiveGarnishmentException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveGarnishmentException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive garnishment exceptions. Empty fields match all.</summary>
public sealed record AdaptiveGarnishmentExceptionQuery(string Department, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveGarnishmentException item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveGarnishmentException> Apply(List<AdaptiveGarnishmentException> items)
    {
        var matched = Collections.Filter(items, (AdaptiveGarnishmentException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive garnishment exception store.</summary>
public static class AdaptiveGarnishmentExceptionStore
{
    public static IAdaptiveGarnishmentExceptionStore NewMemoryAdaptiveGarnishmentExceptionStore()
    {
        return new MemoryAdaptiveGarnishmentExceptionStore(
            new Dictionary<string, AdaptiveGarnishmentException>(),
            new List<string>());
    }

    public static IAdaptiveGarnishmentExceptionStore NewCachedAdaptiveGarnishmentExceptionStore(MemoryAdaptiveGarnishmentExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveGarnishmentExceptionStore(
            inner,
            AdaptiveGarnishmentExceptionCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveGarnishmentException> FindAdaptiveGarnishmentExceptions(IAdaptiveGarnishmentExceptionStore store, AdaptiveGarnishmentExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
