namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched retro pay forecasts by code.</summary>
public interface IBatchedRetroPayForecastStore
{
    BatchedRetroPayForecast? Find(string code);
    void Save(BatchedRetroPayForecast item);
    bool Remove(string code);
    List<BatchedRetroPayForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedRetroPayForecastStore(Dictionary<string, BatchedRetroPayForecast> items, List<string> order) : IBatchedRetroPayForecastStore
{
    public Dictionary<string, BatchedRetroPayForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedRetroPayForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedRetroPayForecast item)
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

    public List<BatchedRetroPayForecast> List()
    {
        var outValue = new List<BatchedRetroPayForecast>();
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
public sealed class CachedBatchedRetroPayForecastStore(MemoryBatchedRetroPayForecastStore inner, BatchedRetroPayForecastCache cache, int hits, int misses) : IBatchedRetroPayForecastStore
{
    public MemoryBatchedRetroPayForecastStore Inner { get; set; } = inner;
    public BatchedRetroPayForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedRetroPayForecast? Find(string code)
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

    public void Save(BatchedRetroPayForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedRetroPayForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched retro pay forecasts. Empty fields match all.</summary>
public sealed record BatchedRetroPayForecastQuery(string Locale, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedRetroPayForecast item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<BatchedRetroPayForecast> Apply(List<BatchedRetroPayForecast> items)
    {
        var matched = Collections.Filter(items, (BatchedRetroPayForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched retro pay forecast store.</summary>
public static class BatchedRetroPayForecastStore
{
    public static IBatchedRetroPayForecastStore NewMemoryBatchedRetroPayForecastStore()
    {
        return new MemoryBatchedRetroPayForecastStore(
            new Dictionary<string, BatchedRetroPayForecast>(),
            new List<string>());
    }

    public static IBatchedRetroPayForecastStore NewCachedBatchedRetroPayForecastStore(MemoryBatchedRetroPayForecastStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedRetroPayForecastStore(
            inner,
            BatchedRetroPayForecastCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<BatchedRetroPayForecast> FindBatchedRetroPayForecasts(IBatchedRetroPayForecastStore store, BatchedRetroPayForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
