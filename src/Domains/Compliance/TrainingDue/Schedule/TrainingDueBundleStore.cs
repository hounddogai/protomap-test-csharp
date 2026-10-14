namespace Acme.Workforce.Compliance.TrainingDue.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due bundles by code.</summary>
public interface ITrainingDueBundleStore
{
    TrainingDueBundle? Find(string code);
    void Save(TrainingDueBundle item);
    bool Remove(string code);
    List<TrainingDueBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueBundleStore(Dictionary<string, TrainingDueBundle> items, List<string> order) : ITrainingDueBundleStore
{
    public Dictionary<string, TrainingDueBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueBundle item)
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

    public List<TrainingDueBundle> List()
    {
        var outValue = new List<TrainingDueBundle>();
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
public sealed class CachedTrainingDueBundleStore(MemoryTrainingDueBundleStore inner, TrainingDueBundleCache cache, int hits, int misses) : ITrainingDueBundleStore
{
    public MemoryTrainingDueBundleStore Inner { get; set; } = inner;
    public TrainingDueBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueBundle? Find(string code)
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

    public void Save(TrainingDueBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due bundles. Empty fields match all.</summary>
public sealed record TrainingDueBundleQuery(string Category, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueBundle item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<TrainingDueBundle> Apply(List<TrainingDueBundle> items)
    {
        var matched = Collections.Filter(items, (TrainingDueBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due bundle store.</summary>
public static class TrainingDueBundleStore
{
    public static ITrainingDueBundleStore NewMemoryTrainingDueBundleStore()
    {
        return new MemoryTrainingDueBundleStore(new Dictionary<string, TrainingDueBundle>(), new List<string>());
    }

    public static ITrainingDueBundleStore NewCachedTrainingDueBundleStore(MemoryTrainingDueBundleStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueBundleStore(inner, TrainingDueBundleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TrainingDueBundle> FindTrainingDueBundles(ITrainingDueBundleStore store, TrainingDueBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
