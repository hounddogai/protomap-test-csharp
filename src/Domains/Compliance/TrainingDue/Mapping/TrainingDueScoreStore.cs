namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due scores by code.</summary>
public interface ITrainingDueScoreStore
{
    TrainingDueScore? Find(string code);
    void Save(TrainingDueScore item);
    bool Remove(string code);
    List<TrainingDueScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueScoreStore(Dictionary<string, TrainingDueScore> items, List<string> order) : ITrainingDueScoreStore
{
    public Dictionary<string, TrainingDueScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueScore item)
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

    public List<TrainingDueScore> List()
    {
        var outValue = new List<TrainingDueScore>();
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
public sealed class CachedTrainingDueScoreStore(MemoryTrainingDueScoreStore inner, TrainingDueScoreCache cache, int hits, int misses) : ITrainingDueScoreStore
{
    public MemoryTrainingDueScoreStore Inner { get; set; } = inner;
    public TrainingDueScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueScore? Find(string code)
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

    public void Save(TrainingDueScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due scores. Empty fields match all.</summary>
public sealed record TrainingDueScoreQuery(string Name, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueScore item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<TrainingDueScore> Apply(List<TrainingDueScore> items)
    {
        var matched = Collections.Filter(items, (TrainingDueScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due score store.</summary>
public static class TrainingDueScoreStore
{
    public static ITrainingDueScoreStore NewMemoryTrainingDueScoreStore()
    {
        return new MemoryTrainingDueScoreStore(new Dictionary<string, TrainingDueScore>(), new List<string>());
    }

    public static ITrainingDueScoreStore NewCachedTrainingDueScoreStore(MemoryTrainingDueScoreStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueScoreStore(inner, TrainingDueScoreCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TrainingDueScore> FindTrainingDueScores(ITrainingDueScoreStore store, TrainingDueScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
