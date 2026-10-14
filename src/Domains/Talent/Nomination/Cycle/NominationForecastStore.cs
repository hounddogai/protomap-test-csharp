namespace Acme.Workforce.Talent.Nomination.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination forecasts by code.</summary>
public interface INominationForecastStore
{
    NominationForecast? Find(string code);
    void Save(NominationForecast item);
    bool Remove(string code);
    List<NominationForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationForecastStore(Dictionary<string, NominationForecast> items, List<string> order) : INominationForecastStore
{
    public Dictionary<string, NominationForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationForecast item)
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

    public List<NominationForecast> List()
    {
        var outValue = new List<NominationForecast>();
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
public sealed class CachedNominationForecastStore(MemoryNominationForecastStore inner, NominationForecastCache cache, int hits, int misses) : INominationForecastStore
{
    public MemoryNominationForecastStore Inner { get; set; } = inner;
    public NominationForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationForecast? Find(string code)
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

    public void Save(NominationForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination forecasts. Empty fields match all.</summary>
public sealed record NominationForecastQuery(string Category, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(NominationForecast item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<NominationForecast> Apply(List<NominationForecast> items)
    {
        var matched = Collections.Filter(items, (NominationForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination forecast store.</summary>
public static class NominationForecastStore
{
    public static INominationForecastStore NewMemoryNominationForecastStore()
    {
        return new MemoryNominationForecastStore(new Dictionary<string, NominationForecast>(), new List<string>());
    }

    public static INominationForecastStore NewCachedNominationForecastStore(MemoryNominationForecastStore inner, int ttl, IClock clock)
    {
        return new CachedNominationForecastStore(inner, NominationForecastCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<NominationForecast> FindNominationForecasts(INominationForecastStore store, NominationForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
