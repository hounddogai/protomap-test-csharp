namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback pulse survey segments by code.</summary>
public interface IFallbackPulseSurveySegmentStore
{
    FallbackPulseSurveySegment? Find(string code);
    void Save(FallbackPulseSurveySegment item);
    bool Remove(string code);
    List<FallbackPulseSurveySegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackPulseSurveySegmentStore(Dictionary<string, FallbackPulseSurveySegment> items, List<string> order) : IFallbackPulseSurveySegmentStore
{
    public Dictionary<string, FallbackPulseSurveySegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackPulseSurveySegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackPulseSurveySegment item)
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

    public List<FallbackPulseSurveySegment> List()
    {
        var outValue = new List<FallbackPulseSurveySegment>();
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
public sealed class CachedFallbackPulseSurveySegmentStore(MemoryFallbackPulseSurveySegmentStore inner, FallbackPulseSurveySegmentCache cache, int hits, int misses) : IFallbackPulseSurveySegmentStore
{
    public MemoryFallbackPulseSurveySegmentStore Inner { get; set; } = inner;
    public FallbackPulseSurveySegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackPulseSurveySegment? Find(string code)
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

    public void Save(FallbackPulseSurveySegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackPulseSurveySegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback pulse survey segments. Empty fields match all.</summary>
public sealed record FallbackPulseSurveySegmentQuery(string Name, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackPulseSurveySegment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackPulseSurveySegment> Apply(List<FallbackPulseSurveySegment> items)
    {
        var matched = Collections.Filter(items, (FallbackPulseSurveySegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback pulse survey segment store.</summary>
public static class FallbackPulseSurveySegmentStore
{
    public static IFallbackPulseSurveySegmentStore NewMemoryFallbackPulseSurveySegmentStore()
    {
        return new MemoryFallbackPulseSurveySegmentStore(
            new Dictionary<string, FallbackPulseSurveySegment>(),
            new List<string>());
    }

    public static IFallbackPulseSurveySegmentStore NewCachedFallbackPulseSurveySegmentStore(MemoryFallbackPulseSurveySegmentStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackPulseSurveySegmentStore(
            inner,
            FallbackPulseSurveySegmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<FallbackPulseSurveySegment> FindFallbackPulseSurveySegments(IFallbackPulseSurveySegmentStore store, FallbackPulseSurveySegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
