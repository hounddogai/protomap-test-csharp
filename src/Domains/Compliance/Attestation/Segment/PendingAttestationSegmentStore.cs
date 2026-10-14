namespace Acme.Workforce.Compliance.Attestation.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending attestation segments by code.</summary>
public interface IPendingAttestationSegmentStore
{
    PendingAttestationSegment? Find(string code);
    void Save(PendingAttestationSegment item);
    bool Remove(string code);
    List<PendingAttestationSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingAttestationSegmentStore(Dictionary<string, PendingAttestationSegment> items, List<string> order) : IPendingAttestationSegmentStore
{
    public Dictionary<string, PendingAttestationSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingAttestationSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingAttestationSegment item)
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

    public List<PendingAttestationSegment> List()
    {
        var outValue = new List<PendingAttestationSegment>();
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
public sealed class CachedPendingAttestationSegmentStore(MemoryPendingAttestationSegmentStore inner, PendingAttestationSegmentCache cache, int hits, int misses) : IPendingAttestationSegmentStore
{
    public MemoryPendingAttestationSegmentStore Inner { get; set; } = inner;
    public PendingAttestationSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingAttestationSegment? Find(string code)
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

    public void Save(PendingAttestationSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingAttestationSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending attestation segments. Empty fields match all.</summary>
public sealed record PendingAttestationSegmentQuery(string Title, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(PendingAttestationSegment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingAttestationSegment> Apply(List<PendingAttestationSegment> items)
    {
        var matched = Collections.Filter(items, (PendingAttestationSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending attestation segment store.</summary>
public static class PendingAttestationSegmentStore
{
    public static IPendingAttestationSegmentStore NewMemoryPendingAttestationSegmentStore()
    {
        return new MemoryPendingAttestationSegmentStore(
            new Dictionary<string, PendingAttestationSegment>(),
            new List<string>());
    }

    public static IPendingAttestationSegmentStore NewCachedPendingAttestationSegmentStore(MemoryPendingAttestationSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedPendingAttestationSegmentStore(
            inner,
            PendingAttestationSegmentCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PendingAttestationSegment> FindPendingAttestationSegments(IPendingAttestationSegmentStore store, PendingAttestationSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
