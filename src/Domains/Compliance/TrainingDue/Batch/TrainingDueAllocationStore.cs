namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due allocations by code.</summary>
public interface ITrainingDueAllocationStore
{
    TrainingDueAllocation? Find(string code);
    void Save(TrainingDueAllocation item);
    bool Remove(string code);
    List<TrainingDueAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueAllocationStore(Dictionary<string, TrainingDueAllocation> items, List<string> order) : ITrainingDueAllocationStore
{
    public Dictionary<string, TrainingDueAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueAllocation item)
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

    public List<TrainingDueAllocation> List()
    {
        var outValue = new List<TrainingDueAllocation>();
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
public sealed class CachedTrainingDueAllocationStore(MemoryTrainingDueAllocationStore inner, TrainingDueAllocationCache cache, int hits, int misses) : ITrainingDueAllocationStore
{
    public MemoryTrainingDueAllocationStore Inner { get; set; } = inner;
    public TrainingDueAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueAllocation? Find(string code)
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

    public void Save(TrainingDueAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due allocations. Empty fields match all.</summary>
public sealed record TrainingDueAllocationQuery(string Title, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueAllocation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TrainingDueAllocation> Apply(List<TrainingDueAllocation> items)
    {
        var matched = Collections.Filter(items, (TrainingDueAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due allocation store.</summary>
public static class TrainingDueAllocationStore
{
    public static ITrainingDueAllocationStore NewMemoryTrainingDueAllocationStore()
    {
        return new MemoryTrainingDueAllocationStore(
            new Dictionary<string, TrainingDueAllocation>(),
            new List<string>());
    }

    public static ITrainingDueAllocationStore NewCachedTrainingDueAllocationStore(MemoryTrainingDueAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueAllocationStore(inner, TrainingDueAllocationCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TrainingDueAllocation> FindTrainingDueAllocations(ITrainingDueAllocationStore store, TrainingDueAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
