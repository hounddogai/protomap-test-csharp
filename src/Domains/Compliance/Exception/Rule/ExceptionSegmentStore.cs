namespace Acme.Workforce.Compliance.Exception.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception segments by code.</summary>
public interface IExceptionSegmentStore
{
    ExceptionSegment? Find(string code);
    void Save(ExceptionSegment item);
    bool Remove(string code);
    List<ExceptionSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionSegmentStore(Dictionary<string, ExceptionSegment> items, List<string> order) : IExceptionSegmentStore
{
    public Dictionary<string, ExceptionSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionSegment item)
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

    public List<ExceptionSegment> List()
    {
        var outValue = new List<ExceptionSegment>();
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
public sealed class CachedExceptionSegmentStore(MemoryExceptionSegmentStore inner, ExceptionSegmentCache cache, int hits, int misses) : IExceptionSegmentStore
{
    public MemoryExceptionSegmentStore Inner { get; set; } = inner;
    public ExceptionSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionSegment? Find(string code)
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

    public void Save(ExceptionSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception segments. Empty fields match all.</summary>
public sealed record ExceptionSegmentQuery(string Source, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionSegment item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ExceptionSegment> Apply(List<ExceptionSegment> items)
    {
        var matched = Collections.Filter(items, (ExceptionSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception segment store.</summary>
public static class ExceptionSegmentStore
{
    public static IExceptionSegmentStore NewMemoryExceptionSegmentStore()
    {
        return new MemoryExceptionSegmentStore(new Dictionary<string, ExceptionSegment>(), new List<string>());
    }

    public static IExceptionSegmentStore NewCachedExceptionSegmentStore(MemoryExceptionSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionSegmentStore(inner, ExceptionSegmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ExceptionSegment> FindExceptionSegments(IExceptionSegmentStore store, ExceptionSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
