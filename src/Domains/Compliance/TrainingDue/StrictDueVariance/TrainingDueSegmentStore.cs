namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due segments by code.</summary>
public interface ITrainingDueSegmentStore
{
    TrainingDueSegment? Find(string code);
    void Save(TrainingDueSegment item);
    bool Remove(string code);
    List<TrainingDueSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueSegmentStore(Dictionary<string, TrainingDueSegment> items, List<string> order) : ITrainingDueSegmentStore
{
    public Dictionary<string, TrainingDueSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueSegment item)
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

    public List<TrainingDueSegment> List()
    {
        var outValue = new List<TrainingDueSegment>();
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
public sealed class CachedTrainingDueSegmentStore(MemoryTrainingDueSegmentStore inner, TrainingDueSegmentCache cache, int hits, int misses) : ITrainingDueSegmentStore
{
    public MemoryTrainingDueSegmentStore Inner { get; set; } = inner;
    public TrainingDueSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueSegment? Find(string code)
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

    public void Save(TrainingDueSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due segments. Empty fields match all.</summary>
public sealed record TrainingDueSegmentQuery(string Country, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueSegment item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<TrainingDueSegment> Apply(List<TrainingDueSegment> items)
    {
        var matched = Collections.Filter(items, (TrainingDueSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due segment store.</summary>
public static class TrainingDueSegmentStore
{
    public static ITrainingDueSegmentStore NewMemoryTrainingDueSegmentStore()
    {
        return new MemoryTrainingDueSegmentStore(new Dictionary<string, TrainingDueSegment>(), new List<string>());
    }

    public static ITrainingDueSegmentStore NewCachedTrainingDueSegmentStore(MemoryTrainingDueSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueSegmentStore(inner, TrainingDueSegmentCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<TrainingDueSegment> FindTrainingDueSegments(ITrainingDueSegmentStore store, TrainingDueSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
