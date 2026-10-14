namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted training due profiles by code.</summary>
public interface IWeightedTrainingDueProfileStore
{
    WeightedTrainingDueProfile? Find(string code);
    void Save(WeightedTrainingDueProfile item);
    bool Remove(string code);
    List<WeightedTrainingDueProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedTrainingDueProfileStore(Dictionary<string, WeightedTrainingDueProfile> items, List<string> order) : IWeightedTrainingDueProfileStore
{
    public Dictionary<string, WeightedTrainingDueProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedTrainingDueProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedTrainingDueProfile item)
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

    public List<WeightedTrainingDueProfile> List()
    {
        var outValue = new List<WeightedTrainingDueProfile>();
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
public sealed class CachedWeightedTrainingDueProfileStore(MemoryWeightedTrainingDueProfileStore inner, WeightedTrainingDueProfileCache cache, int hits, int misses) : IWeightedTrainingDueProfileStore
{
    public MemoryWeightedTrainingDueProfileStore Inner { get; set; } = inner;
    public WeightedTrainingDueProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedTrainingDueProfile? Find(string code)
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

    public void Save(WeightedTrainingDueProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedTrainingDueProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted training due profiles. Empty fields match all.</summary>
public sealed record WeightedTrainingDueProfileQuery(string Title, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedTrainingDueProfile item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedTrainingDueProfile> Apply(List<WeightedTrainingDueProfile> items)
    {
        var matched = Collections.Filter(items, (WeightedTrainingDueProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted training due profile store.</summary>
public static class WeightedTrainingDueProfileStore
{
    public static IWeightedTrainingDueProfileStore NewMemoryWeightedTrainingDueProfileStore()
    {
        return new MemoryWeightedTrainingDueProfileStore(
            new Dictionary<string, WeightedTrainingDueProfile>(),
            new List<string>());
    }

    public static IWeightedTrainingDueProfileStore NewCachedWeightedTrainingDueProfileStore(MemoryWeightedTrainingDueProfileStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedTrainingDueProfileStore(
            inner,
            WeightedTrainingDueProfileCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<WeightedTrainingDueProfile> FindWeightedTrainingDueProfiles(IWeightedTrainingDueProfileStore store, WeightedTrainingDueProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
