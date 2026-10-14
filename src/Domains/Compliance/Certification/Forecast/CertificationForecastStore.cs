namespace Acme.Workforce.Compliance.Certification.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps certification forecasts by code.</summary>
public interface ICertificationForecastStore
{
    CertificationForecast? Find(string code);
    void Save(CertificationForecast item);
    bool Remove(string code);
    List<CertificationForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCertificationForecastStore(Dictionary<string, CertificationForecast> items, List<string> order) : ICertificationForecastStore
{
    public Dictionary<string, CertificationForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CertificationForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CertificationForecast item)
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

    public List<CertificationForecast> List()
    {
        var outValue = new List<CertificationForecast>();
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
public sealed class CachedCertificationForecastStore(MemoryCertificationForecastStore inner, CertificationForecastCache cache, int hits, int misses) : ICertificationForecastStore
{
    public MemoryCertificationForecastStore Inner { get; set; } = inner;
    public CertificationForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CertificationForecast? Find(string code)
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

    public void Save(CertificationForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CertificationForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects certification forecasts. Empty fields match all.</summary>
public sealed record CertificationForecastQuery(string Name, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(CertificationForecast item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<CertificationForecast> Apply(List<CertificationForecast> items)
    {
        var matched = Collections.Filter(items, (CertificationForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of certification forecast store.</summary>
public static class CertificationForecastStore
{
    public static ICertificationForecastStore NewMemoryCertificationForecastStore()
    {
        return new MemoryCertificationForecastStore(
            new Dictionary<string, CertificationForecast>(),
            new List<string>());
    }

    public static ICertificationForecastStore NewCachedCertificationForecastStore(MemoryCertificationForecastStore inner, int ttl, IClock clock)
    {
        return new CachedCertificationForecastStore(inner, CertificationForecastCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CertificationForecast> FindCertificationForecasts(ICertificationForecastStore store, CertificationForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
