namespace Acme.Workforce.Assets.Warranty.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient warranty forecasts by code.</summary>
public interface ILenientWarrantyForecastStore
{
    LenientWarrantyForecast? Find(string code);
    void Save(LenientWarrantyForecast item);
    bool Remove(string code);
    List<LenientWarrantyForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientWarrantyForecastStore(Dictionary<string, LenientWarrantyForecast> items, List<string> order) : ILenientWarrantyForecastStore
{
    public Dictionary<string, LenientWarrantyForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientWarrantyForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientWarrantyForecast item)
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

    public List<LenientWarrantyForecast> List()
    {
        var outValue = new List<LenientWarrantyForecast>();
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
public sealed class CachedLenientWarrantyForecastStore(MemoryLenientWarrantyForecastStore inner, LenientWarrantyForecastCache cache, int hits, int misses) : ILenientWarrantyForecastStore
{
    public MemoryLenientWarrantyForecastStore Inner { get; set; } = inner;
    public LenientWarrantyForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientWarrantyForecast? Find(string code)
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

    public void Save(LenientWarrantyForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientWarrantyForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient warranty forecasts. Empty fields match all.</summary>
public sealed record LenientWarrantyForecastQuery(string Label, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(LenientWarrantyForecast item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientWarrantyForecast> Apply(List<LenientWarrantyForecast> items)
    {
        var matched = Collections.Filter(items, (LenientWarrantyForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient warranty forecast store.</summary>
public static class LenientWarrantyForecastStore
{
    public static ILenientWarrantyForecastStore NewMemoryLenientWarrantyForecastStore()
    {
        return new MemoryLenientWarrantyForecastStore(
            new Dictionary<string, LenientWarrantyForecast>(),
            new List<string>());
    }

    public static ILenientWarrantyForecastStore NewCachedLenientWarrantyForecastStore(MemoryLenientWarrantyForecastStore inner, int ttl, IClock clock)
    {
        return new CachedLenientWarrantyForecastStore(
            inner,
            LenientWarrantyForecastCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LenientWarrantyForecast> FindLenientWarrantyForecasts(ILenientWarrantyForecastStore store, LenientWarrantyForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
