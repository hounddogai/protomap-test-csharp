namespace Acme.Workforce.Compensation.EquityGrant.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant segments by code.</summary>
public interface IEquityGrantSegmentStore
{
    EquityGrantSegment? Find(string code);
    void Save(EquityGrantSegment item);
    bool Remove(string code);
    List<EquityGrantSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantSegmentStore(Dictionary<string, EquityGrantSegment> items, List<string> order) : IEquityGrantSegmentStore
{
    public Dictionary<string, EquityGrantSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantSegment item)
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

    public List<EquityGrantSegment> List()
    {
        var outValue = new List<EquityGrantSegment>();
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
public sealed class CachedEquityGrantSegmentStore(MemoryEquityGrantSegmentStore inner, EquityGrantSegmentCache cache, int hits, int misses) : IEquityGrantSegmentStore
{
    public MemoryEquityGrantSegmentStore Inner { get; set; } = inner;
    public EquityGrantSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantSegment? Find(string code)
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

    public void Save(EquityGrantSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant segments. Empty fields match all.</summary>
public sealed record EquityGrantSegmentQuery(string Locale, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantSegment item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantSegment> Apply(List<EquityGrantSegment> items)
    {
        var matched = Collections.Filter(items, (EquityGrantSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant segment store.</summary>
public static class EquityGrantSegmentStore
{
    public static IEquityGrantSegmentStore NewMemoryEquityGrantSegmentStore()
    {
        return new MemoryEquityGrantSegmentStore(new Dictionary<string, EquityGrantSegment>(), new List<string>());
    }

    public static IEquityGrantSegmentStore NewCachedEquityGrantSegmentStore(MemoryEquityGrantSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantSegmentStore(inner, EquityGrantSegmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<EquityGrantSegment> FindEquityGrantSegments(IEquityGrantSegmentStore store, EquityGrantSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
