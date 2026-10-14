namespace Acme.Workforce.Compliance.Certification.CertificationForecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual certification forecasts by code.</summary>
public interface IManualCertificationForecastStore
{
    ManualCertificationForecast? Find(string code);
    void Save(ManualCertificationForecast item);
    bool Remove(string code);
    List<ManualCertificationForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualCertificationForecastStore(Dictionary<string, ManualCertificationForecast> items, List<string> order) : IManualCertificationForecastStore
{
    public Dictionary<string, ManualCertificationForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualCertificationForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualCertificationForecast item)
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

    public List<ManualCertificationForecast> List()
    {
        var outValue = new List<ManualCertificationForecast>();
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
public sealed class CachedManualCertificationForecastStore(MemoryManualCertificationForecastStore inner, ManualCertificationForecastCache cache, int hits, int misses) : IManualCertificationForecastStore
{
    public MemoryManualCertificationForecastStore Inner { get; set; } = inner;
    public ManualCertificationForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualCertificationForecast? Find(string code)
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

    public void Save(ManualCertificationForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualCertificationForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual certification forecasts. Empty fields match all.</summary>
public sealed record ManualCertificationForecastQuery(string Name, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(ManualCertificationForecast item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualCertificationForecast> Apply(List<ManualCertificationForecast> items)
    {
        var matched = Collections.Filter(items, (ManualCertificationForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual certification forecast store.</summary>
public static class ManualCertificationForecastStore
{
    public static IManualCertificationForecastStore NewMemoryManualCertificationForecastStore()
    {
        return new MemoryManualCertificationForecastStore(
            new Dictionary<string, ManualCertificationForecast>(),
            new List<string>());
    }

    public static IManualCertificationForecastStore NewCachedManualCertificationForecastStore(MemoryManualCertificationForecastStore inner, int ttl, IClock clock)
    {
        return new CachedManualCertificationForecastStore(
            inner,
            ManualCertificationForecastCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ManualCertificationForecast> FindManualCertificationForecasts(IManualCertificationForecastStore store, ManualCertificationForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
