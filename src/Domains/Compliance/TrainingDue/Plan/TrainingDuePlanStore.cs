namespace Acme.Workforce.Compliance.TrainingDue.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due plans by code.</summary>
public interface ITrainingDuePlanStore
{
    TrainingDuePlan? Find(string code);
    void Save(TrainingDuePlan item);
    bool Remove(string code);
    List<TrainingDuePlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDuePlanStore(Dictionary<string, TrainingDuePlan> items, List<string> order) : ITrainingDuePlanStore
{
    public Dictionary<string, TrainingDuePlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDuePlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDuePlan item)
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

    public List<TrainingDuePlan> List()
    {
        var outValue = new List<TrainingDuePlan>();
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
public sealed class CachedTrainingDuePlanStore(MemoryTrainingDuePlanStore inner, TrainingDuePlanCache cache, int hits, int misses) : ITrainingDuePlanStore
{
    public MemoryTrainingDuePlanStore Inner { get; set; } = inner;
    public TrainingDuePlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDuePlan? Find(string code)
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

    public void Save(TrainingDuePlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDuePlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due plans. Empty fields match all.</summary>
public sealed record TrainingDuePlanQuery(string Name, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDuePlan item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TrainingDuePlan> Apply(List<TrainingDuePlan> items)
    {
        var matched = Collections.Filter(items, (TrainingDuePlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due plan store.</summary>
public static class TrainingDuePlanStore
{
    public static ITrainingDuePlanStore NewMemoryTrainingDuePlanStore()
    {
        return new MemoryTrainingDuePlanStore(new Dictionary<string, TrainingDuePlan>(), new List<string>());
    }

    public static ITrainingDuePlanStore NewCachedTrainingDuePlanStore(MemoryTrainingDuePlanStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDuePlanStore(inner, TrainingDuePlanCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TrainingDuePlan> FindTrainingDuePlans(ITrainingDuePlanStore store, TrainingDuePlanQuery query)
    {
        return query.Apply(store.List());
    }
}
