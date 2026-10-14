namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due mappings by code.</summary>
public interface ITrainingDueMappingStore
{
    TrainingDueMapping? Find(string code);
    void Save(TrainingDueMapping item);
    bool Remove(string code);
    List<TrainingDueMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueMappingStore(Dictionary<string, TrainingDueMapping> items, List<string> order) : ITrainingDueMappingStore
{
    public Dictionary<string, TrainingDueMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueMapping item)
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

    public List<TrainingDueMapping> List()
    {
        var outValue = new List<TrainingDueMapping>();
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
public sealed class CachedTrainingDueMappingStore(MemoryTrainingDueMappingStore inner, TrainingDueMappingCache cache, int hits, int misses) : ITrainingDueMappingStore
{
    public MemoryTrainingDueMappingStore Inner { get; set; } = inner;
    public TrainingDueMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueMapping? Find(string code)
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

    public void Save(TrainingDueMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due mappings. Empty fields match all.</summary>
public sealed record TrainingDueMappingQuery(string Segment, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueMapping item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TrainingDueMapping> Apply(List<TrainingDueMapping> items)
    {
        var matched = Collections.Filter(items, (TrainingDueMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due mapping store.</summary>
public static class TrainingDueMappingStore
{
    public static ITrainingDueMappingStore NewMemoryTrainingDueMappingStore()
    {
        return new MemoryTrainingDueMappingStore(new Dictionary<string, TrainingDueMapping>(), new List<string>());
    }

    public static ITrainingDueMappingStore NewCachedTrainingDueMappingStore(MemoryTrainingDueMappingStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueMappingStore(inner, TrainingDueMappingCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TrainingDueMapping> FindTrainingDueMappings(ITrainingDueMappingStore store, TrainingDueMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
