namespace Acme.Workforce.Absence.Policy.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy forecasts by code.</summary>
public interface IPolicyForecastStore
{
    PolicyForecast? Find(string code);
    void Save(PolicyForecast item);
    bool Remove(string code);
    List<PolicyForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyForecastStore(Dictionary<string, PolicyForecast> items, List<string> order) : IPolicyForecastStore
{
    public Dictionary<string, PolicyForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyForecast item)
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

    public List<PolicyForecast> List()
    {
        var outValue = new List<PolicyForecast>();
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
public sealed class CachedPolicyForecastStore(MemoryPolicyForecastStore inner, PolicyForecastCache cache, int hits, int misses) : IPolicyForecastStore
{
    public MemoryPolicyForecastStore Inner { get; set; } = inner;
    public PolicyForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyForecast? Find(string code)
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

    public void Save(PolicyForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy forecasts. Empty fields match all.</summary>
public sealed record PolicyForecastQuery(string Title, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyForecast item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyForecast> Apply(List<PolicyForecast> items)
    {
        var matched = Collections.Filter(items, (PolicyForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy forecast store.</summary>
public static class PolicyForecastStore
{
    public static IPolicyForecastStore NewMemoryPolicyForecastStore()
    {
        return new MemoryPolicyForecastStore(new Dictionary<string, PolicyForecast>(), new List<string>());
    }

    public static IPolicyForecastStore NewCachedPolicyForecastStore(MemoryPolicyForecastStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyForecastStore(inner, PolicyForecastCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PolicyForecast> FindPolicyForecasts(IPolicyForecastStore store, PolicyForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
