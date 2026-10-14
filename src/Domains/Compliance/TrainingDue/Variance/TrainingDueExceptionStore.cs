namespace Acme.Workforce.Compliance.TrainingDue.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due exceptions by code.</summary>
public interface ITrainingDueExceptionStore
{
    TrainingDueException? Find(string code);
    void Save(TrainingDueException item);
    bool Remove(string code);
    List<TrainingDueException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueExceptionStore(Dictionary<string, TrainingDueException> items, List<string> order) : ITrainingDueExceptionStore
{
    public Dictionary<string, TrainingDueException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueException item)
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

    public List<TrainingDueException> List()
    {
        var outValue = new List<TrainingDueException>();
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
public sealed class CachedTrainingDueExceptionStore(MemoryTrainingDueExceptionStore inner, TrainingDueExceptionCache cache, int hits, int misses) : ITrainingDueExceptionStore
{
    public MemoryTrainingDueExceptionStore Inner { get; set; } = inner;
    public TrainingDueExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueException? Find(string code)
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

    public void Save(TrainingDueException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due exceptions. Empty fields match all.</summary>
public sealed record TrainingDueExceptionQuery(string Department, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueException item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<TrainingDueException> Apply(List<TrainingDueException> items)
    {
        var matched = Collections.Filter(items, (TrainingDueException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due exception store.</summary>
public static class TrainingDueExceptionStore
{
    public static ITrainingDueExceptionStore NewMemoryTrainingDueExceptionStore()
    {
        return new MemoryTrainingDueExceptionStore(
            new Dictionary<string, TrainingDueException>(),
            new List<string>());
    }

    public static ITrainingDueExceptionStore NewCachedTrainingDueExceptionStore(MemoryTrainingDueExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueExceptionStore(inner, TrainingDueExceptionCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<TrainingDueException> FindTrainingDueExceptions(ITrainingDueExceptionStore store, TrainingDueExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
