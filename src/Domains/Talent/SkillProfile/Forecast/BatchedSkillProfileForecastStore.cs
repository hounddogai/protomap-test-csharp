namespace Acme.Workforce.Talent.SkillProfile.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched skill profile forecasts by code.</summary>
public interface IBatchedSkillProfileForecastStore
{
    BatchedSkillProfileForecast? Find(string code);
    void Save(BatchedSkillProfileForecast item);
    bool Remove(string code);
    List<BatchedSkillProfileForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedSkillProfileForecastStore(Dictionary<string, BatchedSkillProfileForecast> items, List<string> order) : IBatchedSkillProfileForecastStore
{
    public Dictionary<string, BatchedSkillProfileForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedSkillProfileForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedSkillProfileForecast item)
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

    public List<BatchedSkillProfileForecast> List()
    {
        var outValue = new List<BatchedSkillProfileForecast>();
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
public sealed class CachedBatchedSkillProfileForecastStore(MemoryBatchedSkillProfileForecastStore inner, BatchedSkillProfileForecastCache cache, int hits, int misses) : IBatchedSkillProfileForecastStore
{
    public MemoryBatchedSkillProfileForecastStore Inner { get; set; } = inner;
    public BatchedSkillProfileForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedSkillProfileForecast? Find(string code)
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

    public void Save(BatchedSkillProfileForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedSkillProfileForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched skill profile forecasts. Empty fields match all.</summary>
public sealed record BatchedSkillProfileForecastQuery(string Name, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedSkillProfileForecast item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedSkillProfileForecast> Apply(List<BatchedSkillProfileForecast> items)
    {
        var matched = Collections.Filter(items, (BatchedSkillProfileForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched skill profile forecast store.</summary>
public static class BatchedSkillProfileForecastStore
{
    public static IBatchedSkillProfileForecastStore NewMemoryBatchedSkillProfileForecastStore()
    {
        return new MemoryBatchedSkillProfileForecastStore(
            new Dictionary<string, BatchedSkillProfileForecast>(),
            new List<string>());
    }

    public static IBatchedSkillProfileForecastStore NewCachedBatchedSkillProfileForecastStore(MemoryBatchedSkillProfileForecastStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedSkillProfileForecastStore(
            inner,
            BatchedSkillProfileForecastCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<BatchedSkillProfileForecast> FindBatchedSkillProfileForecasts(IBatchedSkillProfileForecastStore store, BatchedSkillProfileForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
