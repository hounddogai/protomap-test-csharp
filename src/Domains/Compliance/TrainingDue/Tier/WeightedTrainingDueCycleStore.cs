namespace Acme.Workforce.Compliance.TrainingDue.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted training due cycles by code.</summary>
public interface IWeightedTrainingDueCycleStore
{
    WeightedTrainingDueCycle? Find(string code);
    void Save(WeightedTrainingDueCycle item);
    bool Remove(string code);
    List<WeightedTrainingDueCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedTrainingDueCycleStore(Dictionary<string, WeightedTrainingDueCycle> items, List<string> order) : IWeightedTrainingDueCycleStore
{
    public Dictionary<string, WeightedTrainingDueCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedTrainingDueCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedTrainingDueCycle item)
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

    public List<WeightedTrainingDueCycle> List()
    {
        var outValue = new List<WeightedTrainingDueCycle>();
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
public sealed class CachedWeightedTrainingDueCycleStore(MemoryWeightedTrainingDueCycleStore inner, WeightedTrainingDueCycleCache cache, int hits, int misses) : IWeightedTrainingDueCycleStore
{
    public MemoryWeightedTrainingDueCycleStore Inner { get; set; } = inner;
    public WeightedTrainingDueCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedTrainingDueCycle? Find(string code)
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

    public void Save(WeightedTrainingDueCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedTrainingDueCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted training due cycles. Empty fields match all.</summary>
public sealed record WeightedTrainingDueCycleQuery(string Label, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedTrainingDueCycle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedTrainingDueCycle> Apply(List<WeightedTrainingDueCycle> items)
    {
        var matched = Collections.Filter(items, (WeightedTrainingDueCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted training due cycle store.</summary>
public static class WeightedTrainingDueCycleStore
{
    public static IWeightedTrainingDueCycleStore NewMemoryWeightedTrainingDueCycleStore()
    {
        return new MemoryWeightedTrainingDueCycleStore(
            new Dictionary<string, WeightedTrainingDueCycle>(),
            new List<string>());
    }

    public static IWeightedTrainingDueCycleStore NewCachedWeightedTrainingDueCycleStore(MemoryWeightedTrainingDueCycleStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedTrainingDueCycleStore(
            inner,
            WeightedTrainingDueCycleCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<WeightedTrainingDueCycle> FindWeightedTrainingDueCycles(IWeightedTrainingDueCycleStore store, WeightedTrainingDueCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
