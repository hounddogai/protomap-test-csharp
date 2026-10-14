namespace Acme.Workforce.Compensation.EquityGrant.GrantBatch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy equity grant segments by code.</summary>
public interface ILegacyEquityGrantSegmentStore
{
    LegacyEquityGrantSegment? Find(string code);
    void Save(LegacyEquityGrantSegment item);
    bool Remove(string code);
    List<LegacyEquityGrantSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyEquityGrantSegmentStore(Dictionary<string, LegacyEquityGrantSegment> items, List<string> order) : ILegacyEquityGrantSegmentStore
{
    public Dictionary<string, LegacyEquityGrantSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyEquityGrantSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyEquityGrantSegment item)
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

    public List<LegacyEquityGrantSegment> List()
    {
        var outValue = new List<LegacyEquityGrantSegment>();
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
public sealed class CachedLegacyEquityGrantSegmentStore(MemoryLegacyEquityGrantSegmentStore inner, LegacyEquityGrantSegmentCache cache, int hits, int misses) : ILegacyEquityGrantSegmentStore
{
    public MemoryLegacyEquityGrantSegmentStore Inner { get; set; } = inner;
    public LegacyEquityGrantSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyEquityGrantSegment? Find(string code)
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

    public void Save(LegacyEquityGrantSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyEquityGrantSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy equity grant segments. Empty fields match all.</summary>
public sealed record LegacyEquityGrantSegmentQuery(string Title, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyEquityGrantSegment item)
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

    public List<LegacyEquityGrantSegment> Apply(List<LegacyEquityGrantSegment> items)
    {
        var matched = Collections.Filter(items, (LegacyEquityGrantSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy equity grant segment store.</summary>
public static class LegacyEquityGrantSegmentStore
{
    public static ILegacyEquityGrantSegmentStore NewMemoryLegacyEquityGrantSegmentStore()
    {
        return new MemoryLegacyEquityGrantSegmentStore(
            new Dictionary<string, LegacyEquityGrantSegment>(),
            new List<string>());
    }

    public static ILegacyEquityGrantSegmentStore NewCachedLegacyEquityGrantSegmentStore(MemoryLegacyEquityGrantSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyEquityGrantSegmentStore(
            inner,
            LegacyEquityGrantSegmentCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LegacyEquityGrantSegment> FindLegacyEquityGrantSegments(ILegacyEquityGrantSegmentStore store, LegacyEquityGrantSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
