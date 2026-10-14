namespace Acme.Workforce.Compliance.Attestation.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation forecasts by code.</summary>
public interface IAttestationForecastStore
{
    AttestationForecast? Find(string code);
    void Save(AttestationForecast item);
    bool Remove(string code);
    List<AttestationForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationForecastStore(Dictionary<string, AttestationForecast> items, List<string> order) : IAttestationForecastStore
{
    public Dictionary<string, AttestationForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationForecast item)
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

    public List<AttestationForecast> List()
    {
        var outValue = new List<AttestationForecast>();
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
public sealed class CachedAttestationForecastStore(MemoryAttestationForecastStore inner, AttestationForecastCache cache, int hits, int misses) : IAttestationForecastStore
{
    public MemoryAttestationForecastStore Inner { get; set; } = inner;
    public AttestationForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationForecast? Find(string code)
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

    public void Save(AttestationForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation forecasts. Empty fields match all.</summary>
public sealed record AttestationForecastQuery(string Label, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationForecast item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationForecast> Apply(List<AttestationForecast> items)
    {
        var matched = Collections.Filter(items, (AttestationForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation forecast store.</summary>
public static class AttestationForecastStore
{
    public static IAttestationForecastStore NewMemoryAttestationForecastStore()
    {
        return new MemoryAttestationForecastStore(new Dictionary<string, AttestationForecast>(), new List<string>());
    }

    public static IAttestationForecastStore NewCachedAttestationForecastStore(MemoryAttestationForecastStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationForecastStore(inner, AttestationForecastCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AttestationForecast> FindAttestationForecasts(IAttestationForecastStore store, AttestationForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
