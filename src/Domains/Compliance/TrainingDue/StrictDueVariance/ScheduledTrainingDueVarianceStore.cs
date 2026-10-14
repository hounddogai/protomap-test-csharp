namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled training due variances by code.</summary>
public interface IScheduledTrainingDueVarianceStore
{
    ScheduledTrainingDueVariance? Find(string code);
    void Save(ScheduledTrainingDueVariance item);
    bool Remove(string code);
    List<ScheduledTrainingDueVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledTrainingDueVarianceStore(Dictionary<string, ScheduledTrainingDueVariance> items, List<string> order) : IScheduledTrainingDueVarianceStore
{
    public Dictionary<string, ScheduledTrainingDueVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledTrainingDueVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledTrainingDueVariance item)
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

    public List<ScheduledTrainingDueVariance> List()
    {
        var outValue = new List<ScheduledTrainingDueVariance>();
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
public sealed class CachedScheduledTrainingDueVarianceStore(MemoryScheduledTrainingDueVarianceStore inner, ScheduledTrainingDueVarianceCache cache, int hits, int misses) : IScheduledTrainingDueVarianceStore
{
    public MemoryScheduledTrainingDueVarianceStore Inner { get; set; } = inner;
    public ScheduledTrainingDueVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledTrainingDueVariance? Find(string code)
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

    public void Save(ScheduledTrainingDueVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledTrainingDueVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled training due variances. Empty fields match all.</summary>
public sealed record ScheduledTrainingDueVarianceQuery(string Category, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledTrainingDueVariance item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<ScheduledTrainingDueVariance> Apply(List<ScheduledTrainingDueVariance> items)
    {
        var matched = Collections.Filter(items, (ScheduledTrainingDueVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled training due variance store.</summary>
public static class ScheduledTrainingDueVarianceStore
{
    public static IScheduledTrainingDueVarianceStore NewMemoryScheduledTrainingDueVarianceStore()
    {
        return new MemoryScheduledTrainingDueVarianceStore(
            new Dictionary<string, ScheduledTrainingDueVariance>(),
            new List<string>());
    }

    public static IScheduledTrainingDueVarianceStore NewCachedScheduledTrainingDueVarianceStore(MemoryScheduledTrainingDueVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledTrainingDueVarianceStore(
            inner,
            ScheduledTrainingDueVarianceCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledTrainingDueVariance> FindScheduledTrainingDueVariances(IScheduledTrainingDueVarianceStore store, ScheduledTrainingDueVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
