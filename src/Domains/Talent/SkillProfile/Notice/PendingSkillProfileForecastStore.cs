namespace Acme.Workforce.Talent.SkillProfile.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending skill profile forecasts by code.</summary>
public interface IPendingSkillProfileForecastStore
{
    PendingSkillProfileForecast? Find(string code);
    void Save(PendingSkillProfileForecast item);
    bool Remove(string code);
    List<PendingSkillProfileForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingSkillProfileForecastStore(Dictionary<string, PendingSkillProfileForecast> items, List<string> order) : IPendingSkillProfileForecastStore
{
    public Dictionary<string, PendingSkillProfileForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingSkillProfileForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingSkillProfileForecast item)
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

    public List<PendingSkillProfileForecast> List()
    {
        var outValue = new List<PendingSkillProfileForecast>();
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
public sealed class CachedPendingSkillProfileForecastStore(MemoryPendingSkillProfileForecastStore inner, PendingSkillProfileForecastCache cache, int hits, int misses) : IPendingSkillProfileForecastStore
{
    public MemoryPendingSkillProfileForecastStore Inner { get; set; } = inner;
    public PendingSkillProfileForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingSkillProfileForecast? Find(string code)
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

    public void Save(PendingSkillProfileForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingSkillProfileForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending skill profile forecasts. Empty fields match all.</summary>
public sealed record PendingSkillProfileForecastQuery(string Title, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(PendingSkillProfileForecast item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingSkillProfileForecast> Apply(List<PendingSkillProfileForecast> items)
    {
        var matched = Collections.Filter(items, (PendingSkillProfileForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending skill profile forecast store.</summary>
public static class PendingSkillProfileForecastStore
{
    public static IPendingSkillProfileForecastStore NewMemoryPendingSkillProfileForecastStore()
    {
        return new MemoryPendingSkillProfileForecastStore(
            new Dictionary<string, PendingSkillProfileForecast>(),
            new List<string>());
    }

    public static IPendingSkillProfileForecastStore NewCachedPendingSkillProfileForecastStore(MemoryPendingSkillProfileForecastStore inner, int ttl, IClock clock)
    {
        return new CachedPendingSkillProfileForecastStore(
            inner,
            PendingSkillProfileForecastCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PendingSkillProfileForecast> FindPendingSkillProfileForecasts(IPendingSkillProfileForecastStore store, PendingSkillProfileForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
