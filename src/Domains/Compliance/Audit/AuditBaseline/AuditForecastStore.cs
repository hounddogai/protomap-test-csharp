namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit forecasts by code.</summary>
public interface IAuditForecastStore
{
    AuditForecast? Find(string code);
    void Save(AuditForecast item);
    bool Remove(string code);
    List<AuditForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditForecastStore(Dictionary<string, AuditForecast> items, List<string> order) : IAuditForecastStore
{
    public Dictionary<string, AuditForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditForecast item)
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

    public List<AuditForecast> List()
    {
        var outValue = new List<AuditForecast>();
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
public sealed class CachedAuditForecastStore(MemoryAuditForecastStore inner, AuditForecastCache cache, int hits, int misses) : IAuditForecastStore
{
    public MemoryAuditForecastStore Inner { get; set; } = inner;
    public AuditForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditForecast? Find(string code)
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

    public void Save(AuditForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit forecasts. Empty fields match all.</summary>
public sealed record AuditForecastQuery(string Unit, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(AuditForecast item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AuditForecast> Apply(List<AuditForecast> items)
    {
        var matched = Collections.Filter(items, (AuditForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit forecast store.</summary>
public static class AuditForecastStore
{
    public static IAuditForecastStore NewMemoryAuditForecastStore()
    {
        return new MemoryAuditForecastStore(new Dictionary<string, AuditForecast>(), new List<string>());
    }

    public static IAuditForecastStore NewCachedAuditForecastStore(MemoryAuditForecastStore inner, int ttl, IClock clock)
    {
        return new CachedAuditForecastStore(inner, AuditForecastCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AuditForecast> FindAuditForecasts(IAuditForecastStore store, AuditForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
