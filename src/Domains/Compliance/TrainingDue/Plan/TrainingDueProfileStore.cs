namespace Acme.Workforce.Compliance.TrainingDue.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due profiles by code.</summary>
public interface ITrainingDueProfileStore
{
    TrainingDueProfile? Find(string code);
    void Save(TrainingDueProfile item);
    bool Remove(string code);
    List<TrainingDueProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueProfileStore(Dictionary<string, TrainingDueProfile> items, List<string> order) : ITrainingDueProfileStore
{
    public Dictionary<string, TrainingDueProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueProfile item)
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

    public List<TrainingDueProfile> List()
    {
        var outValue = new List<TrainingDueProfile>();
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
public sealed class CachedTrainingDueProfileStore(MemoryTrainingDueProfileStore inner, TrainingDueProfileCache cache, int hits, int misses) : ITrainingDueProfileStore
{
    public MemoryTrainingDueProfileStore Inner { get; set; } = inner;
    public TrainingDueProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueProfile? Find(string code)
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

    public void Save(TrainingDueProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due profiles. Empty fields match all.</summary>
public sealed record TrainingDueProfileQuery(string Label, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueProfile item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TrainingDueProfile> Apply(List<TrainingDueProfile> items)
    {
        var matched = Collections.Filter(items, (TrainingDueProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due profile store.</summary>
public static class TrainingDueProfileStore
{
    public static ITrainingDueProfileStore NewMemoryTrainingDueProfileStore()
    {
        return new MemoryTrainingDueProfileStore(new Dictionary<string, TrainingDueProfile>(), new List<string>());
    }

    public static ITrainingDueProfileStore NewCachedTrainingDueProfileStore(MemoryTrainingDueProfileStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueProfileStore(inner, TrainingDueProfileCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<TrainingDueProfile> FindTrainingDueProfiles(ITrainingDueProfileStore store, TrainingDueProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
