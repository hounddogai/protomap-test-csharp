namespace Acme.Workforce.Talent.SkillProfile.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default skill profile forecasts by code.</summary>
public interface IDefaultSkillProfileForecastStore
{
    DefaultSkillProfileForecast? Find(string code);
    void Save(DefaultSkillProfileForecast item);
    bool Remove(string code);
    List<DefaultSkillProfileForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultSkillProfileForecastStore(Dictionary<string, DefaultSkillProfileForecast> items, List<string> order) : IDefaultSkillProfileForecastStore
{
    public Dictionary<string, DefaultSkillProfileForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultSkillProfileForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultSkillProfileForecast item)
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

    public List<DefaultSkillProfileForecast> List()
    {
        var outValue = new List<DefaultSkillProfileForecast>();
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
public sealed class CachedDefaultSkillProfileForecastStore(MemoryDefaultSkillProfileForecastStore inner, DefaultSkillProfileForecastCache cache, int hits, int misses) : IDefaultSkillProfileForecastStore
{
    public MemoryDefaultSkillProfileForecastStore Inner { get; set; } = inner;
    public DefaultSkillProfileForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultSkillProfileForecast? Find(string code)
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

    public void Save(DefaultSkillProfileForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultSkillProfileForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default skill profile forecasts. Empty fields match all.</summary>
public sealed record DefaultSkillProfileForecastQuery(string Team, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultSkillProfileForecast item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultSkillProfileForecast> Apply(List<DefaultSkillProfileForecast> items)
    {
        var matched = Collections.Filter(items, (DefaultSkillProfileForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default skill profile forecast store.</summary>
public static class DefaultSkillProfileForecastStore
{
    public static IDefaultSkillProfileForecastStore NewMemoryDefaultSkillProfileForecastStore()
    {
        return new MemoryDefaultSkillProfileForecastStore(
            new Dictionary<string, DefaultSkillProfileForecast>(),
            new List<string>());
    }

    public static IDefaultSkillProfileForecastStore NewCachedDefaultSkillProfileForecastStore(MemoryDefaultSkillProfileForecastStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultSkillProfileForecastStore(
            inner,
            DefaultSkillProfileForecastCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<DefaultSkillProfileForecast> FindDefaultSkillProfileForecasts(IDefaultSkillProfileForecastStore store, DefaultSkillProfileForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
