namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool forecasts by code.</summary>
public interface IBonusPoolForecastStore
{
    BonusPoolForecast? Find(string code);
    void Save(BonusPoolForecast item);
    bool Remove(string code);
    List<BonusPoolForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolForecastStore(Dictionary<string, BonusPoolForecast> items, List<string> order) : IBonusPoolForecastStore
{
    public Dictionary<string, BonusPoolForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolForecast item)
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

    public List<BonusPoolForecast> List()
    {
        var outValue = new List<BonusPoolForecast>();
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
public sealed class CachedBonusPoolForecastStore(MemoryBonusPoolForecastStore inner, BonusPoolForecastCache cache, int hits, int misses) : IBonusPoolForecastStore
{
    public MemoryBonusPoolForecastStore Inner { get; set; } = inner;
    public BonusPoolForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolForecast? Find(string code)
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

    public void Save(BonusPoolForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool forecasts. Empty fields match all.</summary>
public sealed record BonusPoolForecastQuery(string Title, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolForecast item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolForecast> Apply(List<BonusPoolForecast> items)
    {
        var matched = Collections.Filter(items, (BonusPoolForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool forecast store.</summary>
public static class BonusPoolForecastStore
{
    public static IBonusPoolForecastStore NewMemoryBonusPoolForecastStore()
    {
        return new MemoryBonusPoolForecastStore(new Dictionary<string, BonusPoolForecast>(), new List<string>());
    }

    public static IBonusPoolForecastStore NewCachedBonusPoolForecastStore(MemoryBonusPoolForecastStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolForecastStore(inner, BonusPoolForecastCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BonusPoolForecast> FindBonusPoolForecasts(IBonusPoolForecastStore store, BonusPoolForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
