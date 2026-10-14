namespace Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling pulse survey segments by code.</summary>
public interface IRollingPulseSurveySegmentStore
{
    RollingPulseSurveySegment? Find(string code);
    void Save(RollingPulseSurveySegment item);
    bool Remove(string code);
    List<RollingPulseSurveySegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingPulseSurveySegmentStore(Dictionary<string, RollingPulseSurveySegment> items, List<string> order) : IRollingPulseSurveySegmentStore
{
    public Dictionary<string, RollingPulseSurveySegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingPulseSurveySegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingPulseSurveySegment item)
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

    public List<RollingPulseSurveySegment> List()
    {
        var outValue = new List<RollingPulseSurveySegment>();
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
public sealed class CachedRollingPulseSurveySegmentStore(MemoryRollingPulseSurveySegmentStore inner, RollingPulseSurveySegmentCache cache, int hits, int misses) : IRollingPulseSurveySegmentStore
{
    public MemoryRollingPulseSurveySegmentStore Inner { get; set; } = inner;
    public RollingPulseSurveySegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingPulseSurveySegment? Find(string code)
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

    public void Save(RollingPulseSurveySegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingPulseSurveySegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling pulse survey segments. Empty fields match all.</summary>
public sealed record RollingPulseSurveySegmentQuery(string Segment, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(RollingPulseSurveySegment item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingPulseSurveySegment> Apply(List<RollingPulseSurveySegment> items)
    {
        var matched = Collections.Filter(items, (RollingPulseSurveySegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling pulse survey segment store.</summary>
public static class RollingPulseSurveySegmentStore
{
    public static IRollingPulseSurveySegmentStore NewMemoryRollingPulseSurveySegmentStore()
    {
        return new MemoryRollingPulseSurveySegmentStore(
            new Dictionary<string, RollingPulseSurveySegment>(),
            new List<string>());
    }

    public static IRollingPulseSurveySegmentStore NewCachedRollingPulseSurveySegmentStore(MemoryRollingPulseSurveySegmentStore inner, int ttl, IClock clock)
    {
        return new CachedRollingPulseSurveySegmentStore(
            inner,
            RollingPulseSurveySegmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RollingPulseSurveySegment> FindRollingPulseSurveySegments(IRollingPulseSurveySegmentStore store, RollingPulseSurveySegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
