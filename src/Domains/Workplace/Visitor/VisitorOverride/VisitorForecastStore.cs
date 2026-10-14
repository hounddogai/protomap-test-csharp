namespace Acme.Workforce.Workplace.Visitor.VisitorOverride;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor forecasts by code.</summary>
public interface IVisitorForecastStore
{
    VisitorForecast? Find(string code);
    void Save(VisitorForecast item);
    bool Remove(string code);
    List<VisitorForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorForecastStore(Dictionary<string, VisitorForecast> items, List<string> order) : IVisitorForecastStore
{
    public Dictionary<string, VisitorForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorForecast item)
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

    public List<VisitorForecast> List()
    {
        var outValue = new List<VisitorForecast>();
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
public sealed class CachedVisitorForecastStore(MemoryVisitorForecastStore inner, VisitorForecastCache cache, int hits, int misses) : IVisitorForecastStore
{
    public MemoryVisitorForecastStore Inner { get; set; } = inner;
    public VisitorForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorForecast? Find(string code)
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

    public void Save(VisitorForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor forecasts. Empty fields match all.</summary>
public sealed record VisitorForecastQuery(string Locale, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorForecast item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorForecast> Apply(List<VisitorForecast> items)
    {
        var matched = Collections.Filter(items, (VisitorForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor forecast store.</summary>
public static class VisitorForecastStore
{
    public static IVisitorForecastStore NewMemoryVisitorForecastStore()
    {
        return new MemoryVisitorForecastStore(new Dictionary<string, VisitorForecast>(), new List<string>());
    }

    public static IVisitorForecastStore NewCachedVisitorForecastStore(MemoryVisitorForecastStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorForecastStore(inner, VisitorForecastCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<VisitorForecast> FindVisitorForecasts(IVisitorForecastStore store, VisitorForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
