namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey segments by code.</summary>
public interface IPulseSurveySegmentStore
{
    PulseSurveySegment? Find(string code);
    void Save(PulseSurveySegment item);
    bool Remove(string code);
    List<PulseSurveySegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveySegmentStore(Dictionary<string, PulseSurveySegment> items, List<string> order) : IPulseSurveySegmentStore
{
    public Dictionary<string, PulseSurveySegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveySegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveySegment item)
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

    public List<PulseSurveySegment> List()
    {
        var outValue = new List<PulseSurveySegment>();
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
public sealed class CachedPulseSurveySegmentStore(MemoryPulseSurveySegmentStore inner, PulseSurveySegmentCache cache, int hits, int misses) : IPulseSurveySegmentStore
{
    public MemoryPulseSurveySegmentStore Inner { get; set; } = inner;
    public PulseSurveySegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveySegment? Find(string code)
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

    public void Save(PulseSurveySegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveySegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey segments. Empty fields match all.</summary>
public sealed record PulseSurveySegmentQuery(string Label, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveySegment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<PulseSurveySegment> Apply(List<PulseSurveySegment> items)
    {
        var matched = Collections.Filter(items, (PulseSurveySegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey segment store.</summary>
public static class PulseSurveySegmentStore
{
    public static IPulseSurveySegmentStore NewMemoryPulseSurveySegmentStore()
    {
        return new MemoryPulseSurveySegmentStore(new Dictionary<string, PulseSurveySegment>(), new List<string>());
    }

    public static IPulseSurveySegmentStore NewCachedPulseSurveySegmentStore(MemoryPulseSurveySegmentStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveySegmentStore(inner, PulseSurveySegmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PulseSurveySegment> FindPulseSurveySegments(IPulseSurveySegmentStore store, PulseSurveySegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
