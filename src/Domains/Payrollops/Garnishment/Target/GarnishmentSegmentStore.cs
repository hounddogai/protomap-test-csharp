namespace Acme.Workforce.Payrollops.Garnishment.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment segments by code.</summary>
public interface IGarnishmentSegmentStore
{
    GarnishmentSegment? Find(string code);
    void Save(GarnishmentSegment item);
    bool Remove(string code);
    List<GarnishmentSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentSegmentStore(Dictionary<string, GarnishmentSegment> items, List<string> order) : IGarnishmentSegmentStore
{
    public Dictionary<string, GarnishmentSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentSegment item)
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

    public List<GarnishmentSegment> List()
    {
        var outValue = new List<GarnishmentSegment>();
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
public sealed class CachedGarnishmentSegmentStore(MemoryGarnishmentSegmentStore inner, GarnishmentSegmentCache cache, int hits, int misses) : IGarnishmentSegmentStore
{
    public MemoryGarnishmentSegmentStore Inner { get; set; } = inner;
    public GarnishmentSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentSegment? Find(string code)
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

    public void Save(GarnishmentSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment segments. Empty fields match all.</summary>
public sealed record GarnishmentSegmentQuery(string Department, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentSegment item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GarnishmentSegment> Apply(List<GarnishmentSegment> items)
    {
        var matched = Collections.Filter(items, (GarnishmentSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment segment store.</summary>
public static class GarnishmentSegmentStore
{
    public static IGarnishmentSegmentStore NewMemoryGarnishmentSegmentStore()
    {
        return new MemoryGarnishmentSegmentStore(new Dictionary<string, GarnishmentSegment>(), new List<string>());
    }

    public static IGarnishmentSegmentStore NewCachedGarnishmentSegmentStore(MemoryGarnishmentSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentSegmentStore(inner, GarnishmentSegmentCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<GarnishmentSegment> FindGarnishmentSegments(IGarnishmentSegmentStore store, GarnishmentSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
