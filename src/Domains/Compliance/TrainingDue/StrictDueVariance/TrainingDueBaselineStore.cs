namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due baselines by code.</summary>
public interface ITrainingDueBaselineStore
{
    TrainingDueBaseline? Find(string code);
    void Save(TrainingDueBaseline item);
    bool Remove(string code);
    List<TrainingDueBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueBaselineStore(Dictionary<string, TrainingDueBaseline> items, List<string> order) : ITrainingDueBaselineStore
{
    public Dictionary<string, TrainingDueBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueBaseline item)
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

    public List<TrainingDueBaseline> List()
    {
        var outValue = new List<TrainingDueBaseline>();
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
public sealed class CachedTrainingDueBaselineStore(MemoryTrainingDueBaselineStore inner, TrainingDueBaselineCache cache, int hits, int misses) : ITrainingDueBaselineStore
{
    public MemoryTrainingDueBaselineStore Inner { get; set; } = inner;
    public TrainingDueBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueBaseline? Find(string code)
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

    public void Save(TrainingDueBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due baselines. Empty fields match all.</summary>
public sealed record TrainingDueBaselineQuery(string Label, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueBaseline item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TrainingDueBaseline> Apply(List<TrainingDueBaseline> items)
    {
        var matched = Collections.Filter(items, (TrainingDueBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due baseline store.</summary>
public static class TrainingDueBaselineStore
{
    public static ITrainingDueBaselineStore NewMemoryTrainingDueBaselineStore()
    {
        return new MemoryTrainingDueBaselineStore(new Dictionary<string, TrainingDueBaseline>(), new List<string>());
    }

    public static ITrainingDueBaselineStore NewCachedTrainingDueBaselineStore(MemoryTrainingDueBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueBaselineStore(inner, TrainingDueBaselineCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TrainingDueBaseline> FindTrainingDueBaselines(ITrainingDueBaselineStore store, TrainingDueBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
