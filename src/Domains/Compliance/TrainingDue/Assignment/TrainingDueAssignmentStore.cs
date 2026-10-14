namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due assignments by code.</summary>
public interface ITrainingDueAssignmentStore
{
    TrainingDueAssignment? Find(string code);
    void Save(TrainingDueAssignment item);
    bool Remove(string code);
    List<TrainingDueAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueAssignmentStore(Dictionary<string, TrainingDueAssignment> items, List<string> order) : ITrainingDueAssignmentStore
{
    public Dictionary<string, TrainingDueAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueAssignment item)
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

    public List<TrainingDueAssignment> List()
    {
        var outValue = new List<TrainingDueAssignment>();
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
public sealed class CachedTrainingDueAssignmentStore(MemoryTrainingDueAssignmentStore inner, TrainingDueAssignmentCache cache, int hits, int misses) : ITrainingDueAssignmentStore
{
    public MemoryTrainingDueAssignmentStore Inner { get; set; } = inner;
    public TrainingDueAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueAssignment? Find(string code)
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

    public void Save(TrainingDueAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due assignments. Empty fields match all.</summary>
public sealed record TrainingDueAssignmentQuery(string Department, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueAssignment item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TrainingDueAssignment> Apply(List<TrainingDueAssignment> items)
    {
        var matched = Collections.Filter(items, (TrainingDueAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due assignment store.</summary>
public static class TrainingDueAssignmentStore
{
    public static ITrainingDueAssignmentStore NewMemoryTrainingDueAssignmentStore()
    {
        return new MemoryTrainingDueAssignmentStore(
            new Dictionary<string, TrainingDueAssignment>(),
            new List<string>());
    }

    public static ITrainingDueAssignmentStore NewCachedTrainingDueAssignmentStore(MemoryTrainingDueAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueAssignmentStore(
            inner,
            TrainingDueAssignmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<TrainingDueAssignment> FindTrainingDueAssignments(ITrainingDueAssignmentStore store, TrainingDueAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
