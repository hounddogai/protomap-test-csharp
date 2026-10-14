namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback training due segments by code.</summary>
public interface IFallbackTrainingDueSegmentStore
{
    FallbackTrainingDueSegment? Find(string code);
    void Save(FallbackTrainingDueSegment item);
    bool Remove(string code);
    List<FallbackTrainingDueSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackTrainingDueSegmentStore(Dictionary<string, FallbackTrainingDueSegment> items, List<string> order) : IFallbackTrainingDueSegmentStore
{
    public Dictionary<string, FallbackTrainingDueSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackTrainingDueSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackTrainingDueSegment item)
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

    public List<FallbackTrainingDueSegment> List()
    {
        var outValue = new List<FallbackTrainingDueSegment>();
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
public sealed class CachedFallbackTrainingDueSegmentStore(MemoryFallbackTrainingDueSegmentStore inner, FallbackTrainingDueSegmentCache cache, int hits, int misses) : IFallbackTrainingDueSegmentStore
{
    public MemoryFallbackTrainingDueSegmentStore Inner { get; set; } = inner;
    public FallbackTrainingDueSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackTrainingDueSegment? Find(string code)
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

    public void Save(FallbackTrainingDueSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackTrainingDueSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback training due segments. Empty fields match all.</summary>
public sealed record FallbackTrainingDueSegmentQuery(string Label, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackTrainingDueSegment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackTrainingDueSegment> Apply(List<FallbackTrainingDueSegment> items)
    {
        var matched = Collections.Filter(items, (FallbackTrainingDueSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback training due segment store.</summary>
public static class FallbackTrainingDueSegmentStore
{
    public static IFallbackTrainingDueSegmentStore NewMemoryFallbackTrainingDueSegmentStore()
    {
        return new MemoryFallbackTrainingDueSegmentStore(
            new Dictionary<string, FallbackTrainingDueSegment>(),
            new List<string>());
    }

    public static IFallbackTrainingDueSegmentStore NewCachedFallbackTrainingDueSegmentStore(MemoryFallbackTrainingDueSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackTrainingDueSegmentStore(
            inner,
            FallbackTrainingDueSegmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<FallbackTrainingDueSegment> FindFallbackTrainingDueSegments(IFallbackTrainingDueSegmentStore store, FallbackTrainingDueSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
