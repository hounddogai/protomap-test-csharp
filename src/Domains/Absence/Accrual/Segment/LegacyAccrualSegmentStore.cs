namespace Acme.Workforce.Absence.Accrual.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy accrual segments by code.</summary>
public interface ILegacyAccrualSegmentStore
{
    LegacyAccrualSegment? Find(string code);
    void Save(LegacyAccrualSegment item);
    bool Remove(string code);
    List<LegacyAccrualSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyAccrualSegmentStore(Dictionary<string, LegacyAccrualSegment> items, List<string> order) : ILegacyAccrualSegmentStore
{
    public Dictionary<string, LegacyAccrualSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyAccrualSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyAccrualSegment item)
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

    public List<LegacyAccrualSegment> List()
    {
        var outValue = new List<LegacyAccrualSegment>();
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
public sealed class CachedLegacyAccrualSegmentStore(MemoryLegacyAccrualSegmentStore inner, LegacyAccrualSegmentCache cache, int hits, int misses) : ILegacyAccrualSegmentStore
{
    public MemoryLegacyAccrualSegmentStore Inner { get; set; } = inner;
    public LegacyAccrualSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyAccrualSegment? Find(string code)
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

    public void Save(LegacyAccrualSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyAccrualSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy accrual segments. Empty fields match all.</summary>
public sealed record LegacyAccrualSegmentQuery(string Title, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyAccrualSegment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyAccrualSegment> Apply(List<LegacyAccrualSegment> items)
    {
        var matched = Collections.Filter(items, (LegacyAccrualSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy accrual segment store.</summary>
public static class LegacyAccrualSegmentStore
{
    public static ILegacyAccrualSegmentStore NewMemoryLegacyAccrualSegmentStore()
    {
        return new MemoryLegacyAccrualSegmentStore(
            new Dictionary<string, LegacyAccrualSegment>(),
            new List<string>());
    }

    public static ILegacyAccrualSegmentStore NewCachedLegacyAccrualSegmentStore(MemoryLegacyAccrualSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyAccrualSegmentStore(inner, LegacyAccrualSegmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LegacyAccrualSegment> FindLegacyAccrualSegments(ILegacyAccrualSegmentStore store, LegacyAccrualSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
