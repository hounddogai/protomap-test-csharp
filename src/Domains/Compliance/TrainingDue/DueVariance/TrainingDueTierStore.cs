namespace Acme.Workforce.Compliance.TrainingDue.DueVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due tiers by code.</summary>
public interface ITrainingDueTierStore
{
    TrainingDueTier? Find(string code);
    void Save(TrainingDueTier item);
    bool Remove(string code);
    List<TrainingDueTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueTierStore(Dictionary<string, TrainingDueTier> items, List<string> order) : ITrainingDueTierStore
{
    public Dictionary<string, TrainingDueTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueTier item)
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

    public List<TrainingDueTier> List()
    {
        var outValue = new List<TrainingDueTier>();
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
public sealed class CachedTrainingDueTierStore(MemoryTrainingDueTierStore inner, TrainingDueTierCache cache, int hits, int misses) : ITrainingDueTierStore
{
    public MemoryTrainingDueTierStore Inner { get; set; } = inner;
    public TrainingDueTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueTier? Find(string code)
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

    public void Save(TrainingDueTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due tiers. Empty fields match all.</summary>
public sealed record TrainingDueTierQuery(string Department, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueTier item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TrainingDueTier> Apply(List<TrainingDueTier> items)
    {
        var matched = Collections.Filter(items, (TrainingDueTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due tier store.</summary>
public static class TrainingDueTierStore
{
    public static ITrainingDueTierStore NewMemoryTrainingDueTierStore()
    {
        return new MemoryTrainingDueTierStore(new Dictionary<string, TrainingDueTier>(), new List<string>());
    }

    public static ITrainingDueTierStore NewCachedTrainingDueTierStore(MemoryTrainingDueTierStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueTierStore(inner, TrainingDueTierCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TrainingDueTier> FindTrainingDueTiers(ITrainingDueTierStore store, TrainingDueTierQuery query)
    {
        return query.Apply(store.List());
    }
}
