namespace Acme.Workforce.Talent.SkillProfile.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped skill profile forecasts by code.</summary>
public interface ICappedSkillProfileForecastStore
{
    CappedSkillProfileForecast? Find(string code);
    void Save(CappedSkillProfileForecast item);
    bool Remove(string code);
    List<CappedSkillProfileForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedSkillProfileForecastStore(Dictionary<string, CappedSkillProfileForecast> items, List<string> order) : ICappedSkillProfileForecastStore
{
    public Dictionary<string, CappedSkillProfileForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedSkillProfileForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedSkillProfileForecast item)
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

    public List<CappedSkillProfileForecast> List()
    {
        var outValue = new List<CappedSkillProfileForecast>();
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
public sealed class CachedCappedSkillProfileForecastStore(MemoryCappedSkillProfileForecastStore inner, CappedSkillProfileForecastCache cache, int hits, int misses) : ICappedSkillProfileForecastStore
{
    public MemoryCappedSkillProfileForecastStore Inner { get; set; } = inner;
    public CappedSkillProfileForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedSkillProfileForecast? Find(string code)
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

    public void Save(CappedSkillProfileForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedSkillProfileForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped skill profile forecasts. Empty fields match all.</summary>
public sealed record CappedSkillProfileForecastQuery(string Team, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(CappedSkillProfileForecast item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<CappedSkillProfileForecast> Apply(List<CappedSkillProfileForecast> items)
    {
        var matched = Collections.Filter(items, (CappedSkillProfileForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped skill profile forecast store.</summary>
public static class CappedSkillProfileForecastStore
{
    public static ICappedSkillProfileForecastStore NewMemoryCappedSkillProfileForecastStore()
    {
        return new MemoryCappedSkillProfileForecastStore(
            new Dictionary<string, CappedSkillProfileForecast>(),
            new List<string>());
    }

    public static ICappedSkillProfileForecastStore NewCachedCappedSkillProfileForecastStore(MemoryCappedSkillProfileForecastStore inner, int ttl, IClock clock)
    {
        return new CachedCappedSkillProfileForecastStore(
            inner,
            CappedSkillProfileForecastCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<CappedSkillProfileForecast> FindCappedSkillProfileForecasts(ICappedSkillProfileForecastStore store, CappedSkillProfileForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
