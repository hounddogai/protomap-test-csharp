namespace Acme.Workforce.Compliance.TrainingDue.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due windows by code.</summary>
public interface ITrainingDueWindowStore
{
    TrainingDueWindow? Find(string code);
    void Save(TrainingDueWindow item);
    bool Remove(string code);
    List<TrainingDueWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueWindowStore(Dictionary<string, TrainingDueWindow> items, List<string> order) : ITrainingDueWindowStore
{
    public Dictionary<string, TrainingDueWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueWindow item)
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

    public List<TrainingDueWindow> List()
    {
        var outValue = new List<TrainingDueWindow>();
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
public sealed class CachedTrainingDueWindowStore(MemoryTrainingDueWindowStore inner, TrainingDueWindowCache cache, int hits, int misses) : ITrainingDueWindowStore
{
    public MemoryTrainingDueWindowStore Inner { get; set; } = inner;
    public TrainingDueWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueWindow? Find(string code)
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

    public void Save(TrainingDueWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due windows. Empty fields match all.</summary>
public sealed record TrainingDueWindowQuery(string Channel, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueWindow item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TrainingDueWindow> Apply(List<TrainingDueWindow> items)
    {
        var matched = Collections.Filter(items, (TrainingDueWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due window store.</summary>
public static class TrainingDueWindowStore
{
    public static ITrainingDueWindowStore NewMemoryTrainingDueWindowStore()
    {
        return new MemoryTrainingDueWindowStore(new Dictionary<string, TrainingDueWindow>(), new List<string>());
    }

    public static ITrainingDueWindowStore NewCachedTrainingDueWindowStore(MemoryTrainingDueWindowStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueWindowStore(inner, TrainingDueWindowCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<TrainingDueWindow> FindTrainingDueWindows(ITrainingDueWindowStore store, TrainingDueWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
