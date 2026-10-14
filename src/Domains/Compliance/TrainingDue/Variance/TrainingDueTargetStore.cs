namespace Acme.Workforce.Compliance.TrainingDue.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due targets by code.</summary>
public interface ITrainingDueTargetStore
{
    TrainingDueTarget? Find(string code);
    void Save(TrainingDueTarget item);
    bool Remove(string code);
    List<TrainingDueTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueTargetStore(Dictionary<string, TrainingDueTarget> items, List<string> order) : ITrainingDueTargetStore
{
    public Dictionary<string, TrainingDueTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueTarget item)
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

    public List<TrainingDueTarget> List()
    {
        var outValue = new List<TrainingDueTarget>();
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
public sealed class CachedTrainingDueTargetStore(MemoryTrainingDueTargetStore inner, TrainingDueTargetCache cache, int hits, int misses) : ITrainingDueTargetStore
{
    public MemoryTrainingDueTargetStore Inner { get; set; } = inner;
    public TrainingDueTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueTarget? Find(string code)
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

    public void Save(TrainingDueTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due targets. Empty fields match all.</summary>
public sealed record TrainingDueTargetQuery(string Name, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueTarget item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TrainingDueTarget> Apply(List<TrainingDueTarget> items)
    {
        var matched = Collections.Filter(items, (TrainingDueTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due target store.</summary>
public static class TrainingDueTargetStore
{
    public static ITrainingDueTargetStore NewMemoryTrainingDueTargetStore()
    {
        return new MemoryTrainingDueTargetStore(new Dictionary<string, TrainingDueTarget>(), new List<string>());
    }

    public static ITrainingDueTargetStore NewCachedTrainingDueTargetStore(MemoryTrainingDueTargetStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueTargetStore(inner, TrainingDueTargetCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TrainingDueTarget> FindTrainingDueTargets(ITrainingDueTargetStore store, TrainingDueTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
