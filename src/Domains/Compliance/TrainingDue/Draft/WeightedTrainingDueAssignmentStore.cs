namespace Acme.Workforce.Compliance.TrainingDue.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted training due assignments by code.</summary>
public interface IWeightedTrainingDueAssignmentStore
{
    WeightedTrainingDueAssignment? Find(string code);
    void Save(WeightedTrainingDueAssignment item);
    bool Remove(string code);
    List<WeightedTrainingDueAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedTrainingDueAssignmentStore(Dictionary<string, WeightedTrainingDueAssignment> items, List<string> order) : IWeightedTrainingDueAssignmentStore
{
    public Dictionary<string, WeightedTrainingDueAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedTrainingDueAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedTrainingDueAssignment item)
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

    public List<WeightedTrainingDueAssignment> List()
    {
        var outValue = new List<WeightedTrainingDueAssignment>();
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
public sealed class CachedWeightedTrainingDueAssignmentStore(MemoryWeightedTrainingDueAssignmentStore inner, WeightedTrainingDueAssignmentCache cache, int hits, int misses) : IWeightedTrainingDueAssignmentStore
{
    public MemoryWeightedTrainingDueAssignmentStore Inner { get; set; } = inner;
    public WeightedTrainingDueAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedTrainingDueAssignment? Find(string code)
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

    public void Save(WeightedTrainingDueAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedTrainingDueAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted training due assignments. Empty fields match all.</summary>
public sealed record WeightedTrainingDueAssignmentQuery(string Team, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedTrainingDueAssignment item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<WeightedTrainingDueAssignment> Apply(List<WeightedTrainingDueAssignment> items)
    {
        var matched = Collections.Filter(items, (WeightedTrainingDueAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted training due assignment store.</summary>
public static class WeightedTrainingDueAssignmentStore
{
    public static IWeightedTrainingDueAssignmentStore NewMemoryWeightedTrainingDueAssignmentStore()
    {
        return new MemoryWeightedTrainingDueAssignmentStore(
            new Dictionary<string, WeightedTrainingDueAssignment>(),
            new List<string>());
    }

    public static IWeightedTrainingDueAssignmentStore NewCachedWeightedTrainingDueAssignmentStore(MemoryWeightedTrainingDueAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedTrainingDueAssignmentStore(
            inner,
            WeightedTrainingDueAssignmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<WeightedTrainingDueAssignment> FindWeightedTrainingDueAssignments(IWeightedTrainingDueAssignmentStore store, WeightedTrainingDueAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
