namespace Acme.Workforce.Compensation.Benchmark.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional benchmark forecasts by code.</summary>
public interface IRegionalBenchmarkForecastStore
{
    RegionalBenchmarkForecast? Find(string code);
    void Save(RegionalBenchmarkForecast item);
    bool Remove(string code);
    List<RegionalBenchmarkForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalBenchmarkForecastStore(Dictionary<string, RegionalBenchmarkForecast> items, List<string> order) : IRegionalBenchmarkForecastStore
{
    public Dictionary<string, RegionalBenchmarkForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalBenchmarkForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalBenchmarkForecast item)
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

    public List<RegionalBenchmarkForecast> List()
    {
        var outValue = new List<RegionalBenchmarkForecast>();
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
public sealed class CachedRegionalBenchmarkForecastStore(MemoryRegionalBenchmarkForecastStore inner, RegionalBenchmarkForecastCache cache, int hits, int misses) : IRegionalBenchmarkForecastStore
{
    public MemoryRegionalBenchmarkForecastStore Inner { get; set; } = inner;
    public RegionalBenchmarkForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalBenchmarkForecast? Find(string code)
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

    public void Save(RegionalBenchmarkForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalBenchmarkForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional benchmark forecasts. Empty fields match all.</summary>
public sealed record RegionalBenchmarkForecastQuery(string Team, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalBenchmarkForecast item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalBenchmarkForecast> Apply(List<RegionalBenchmarkForecast> items)
    {
        var matched = Collections.Filter(items, (RegionalBenchmarkForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional benchmark forecast store.</summary>
public static class RegionalBenchmarkForecastStore
{
    public static IRegionalBenchmarkForecastStore NewMemoryRegionalBenchmarkForecastStore()
    {
        return new MemoryRegionalBenchmarkForecastStore(
            new Dictionary<string, RegionalBenchmarkForecast>(),
            new List<string>());
    }

    public static IRegionalBenchmarkForecastStore NewCachedRegionalBenchmarkForecastStore(MemoryRegionalBenchmarkForecastStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalBenchmarkForecastStore(
            inner,
            RegionalBenchmarkForecastCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<RegionalBenchmarkForecast> FindRegionalBenchmarkForecasts(IRegionalBenchmarkForecastStore store, RegionalBenchmarkForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
