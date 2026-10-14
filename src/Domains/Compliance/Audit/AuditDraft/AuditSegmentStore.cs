namespace Acme.Workforce.Compliance.Audit.AuditDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit segments by code.</summary>
public interface IAuditSegmentStore
{
    AuditSegment? Find(string code);
    void Save(AuditSegment item);
    bool Remove(string code);
    List<AuditSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditSegmentStore(Dictionary<string, AuditSegment> items, List<string> order) : IAuditSegmentStore
{
    public Dictionary<string, AuditSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditSegment item)
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

    public List<AuditSegment> List()
    {
        var outValue = new List<AuditSegment>();
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
public sealed class CachedAuditSegmentStore(MemoryAuditSegmentStore inner, AuditSegmentCache cache, int hits, int misses) : IAuditSegmentStore
{
    public MemoryAuditSegmentStore Inner { get; set; } = inner;
    public AuditSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditSegment? Find(string code)
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

    public void Save(AuditSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit segments. Empty fields match all.</summary>
public sealed record AuditSegmentQuery(string Title, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(AuditSegment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AuditSegment> Apply(List<AuditSegment> items)
    {
        var matched = Collections.Filter(items, (AuditSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit segment store.</summary>
public static class AuditSegmentStore
{
    public static IAuditSegmentStore NewMemoryAuditSegmentStore()
    {
        return new MemoryAuditSegmentStore(new Dictionary<string, AuditSegment>(), new List<string>());
    }

    public static IAuditSegmentStore NewCachedAuditSegmentStore(MemoryAuditSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedAuditSegmentStore(inner, AuditSegmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AuditSegment> FindAuditSegments(IAuditSegmentStore store, AuditSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
