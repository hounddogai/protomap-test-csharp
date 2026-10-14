namespace Acme.Workforce.Compliance.TrainingDue.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due groups by code.</summary>
public interface ITrainingDueGroupStore
{
    TrainingDueGroup? Find(string code);
    void Save(TrainingDueGroup item);
    bool Remove(string code);
    List<TrainingDueGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueGroupStore(Dictionary<string, TrainingDueGroup> items, List<string> order) : ITrainingDueGroupStore
{
    public Dictionary<string, TrainingDueGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueGroup item)
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

    public List<TrainingDueGroup> List()
    {
        var outValue = new List<TrainingDueGroup>();
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
public sealed class CachedTrainingDueGroupStore(MemoryTrainingDueGroupStore inner, TrainingDueGroupCache cache, int hits, int misses) : ITrainingDueGroupStore
{
    public MemoryTrainingDueGroupStore Inner { get; set; } = inner;
    public TrainingDueGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueGroup? Find(string code)
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

    public void Save(TrainingDueGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due groups. Empty fields match all.</summary>
public sealed record TrainingDueGroupQuery(string Title, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueGroup item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TrainingDueGroup> Apply(List<TrainingDueGroup> items)
    {
        var matched = Collections.Filter(items, (TrainingDueGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due group store.</summary>
public static class TrainingDueGroupStore
{
    public static ITrainingDueGroupStore NewMemoryTrainingDueGroupStore()
    {
        return new MemoryTrainingDueGroupStore(new Dictionary<string, TrainingDueGroup>(), new List<string>());
    }

    public static ITrainingDueGroupStore NewCachedTrainingDueGroupStore(MemoryTrainingDueGroupStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueGroupStore(inner, TrainingDueGroupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TrainingDueGroup> FindTrainingDueGroups(ITrainingDueGroupStore store, TrainingDueGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
