namespace Acme.Workforce.Engagement.Recognition.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual recognition segments by code.</summary>
public interface IManualRecognitionSegmentStore
{
    ManualRecognitionSegment? Find(string code);
    void Save(ManualRecognitionSegment item);
    bool Remove(string code);
    List<ManualRecognitionSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualRecognitionSegmentStore(Dictionary<string, ManualRecognitionSegment> items, List<string> order) : IManualRecognitionSegmentStore
{
    public Dictionary<string, ManualRecognitionSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualRecognitionSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualRecognitionSegment item)
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

    public List<ManualRecognitionSegment> List()
    {
        var outValue = new List<ManualRecognitionSegment>();
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
public sealed class CachedManualRecognitionSegmentStore(MemoryManualRecognitionSegmentStore inner, ManualRecognitionSegmentCache cache, int hits, int misses) : IManualRecognitionSegmentStore
{
    public MemoryManualRecognitionSegmentStore Inner { get; set; } = inner;
    public ManualRecognitionSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualRecognitionSegment? Find(string code)
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

    public void Save(ManualRecognitionSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualRecognitionSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual recognition segments. Empty fields match all.</summary>
public sealed record ManualRecognitionSegmentQuery(string Title, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(ManualRecognitionSegment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualRecognitionSegment> Apply(List<ManualRecognitionSegment> items)
    {
        var matched = Collections.Filter(items, (ManualRecognitionSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual recognition segment store.</summary>
public static class ManualRecognitionSegmentStore
{
    public static IManualRecognitionSegmentStore NewMemoryManualRecognitionSegmentStore()
    {
        return new MemoryManualRecognitionSegmentStore(
            new Dictionary<string, ManualRecognitionSegment>(),
            new List<string>());
    }

    public static IManualRecognitionSegmentStore NewCachedManualRecognitionSegmentStore(MemoryManualRecognitionSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedManualRecognitionSegmentStore(
            inner,
            ManualRecognitionSegmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<ManualRecognitionSegment> FindManualRecognitionSegments(IManualRecognitionSegmentStore store, ManualRecognitionSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
