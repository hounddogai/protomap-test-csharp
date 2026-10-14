namespace Acme.Workforce.Compliance.Attestation.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation segments by code.</summary>
public interface IAttestationSegmentStore
{
    AttestationSegment? Find(string code);
    void Save(AttestationSegment item);
    bool Remove(string code);
    List<AttestationSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationSegmentStore(Dictionary<string, AttestationSegment> items, List<string> order) : IAttestationSegmentStore
{
    public Dictionary<string, AttestationSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationSegment item)
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

    public List<AttestationSegment> List()
    {
        var outValue = new List<AttestationSegment>();
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
public sealed class CachedAttestationSegmentStore(MemoryAttestationSegmentStore inner, AttestationSegmentCache cache, int hits, int misses) : IAttestationSegmentStore
{
    public MemoryAttestationSegmentStore Inner { get; set; } = inner;
    public AttestationSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationSegment? Find(string code)
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

    public void Save(AttestationSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation segments. Empty fields match all.</summary>
public sealed record AttestationSegmentQuery(string Name, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationSegment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationSegment> Apply(List<AttestationSegment> items)
    {
        var matched = Collections.Filter(items, (AttestationSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation segment store.</summary>
public static class AttestationSegmentStore
{
    public static IAttestationSegmentStore NewMemoryAttestationSegmentStore()
    {
        return new MemoryAttestationSegmentStore(new Dictionary<string, AttestationSegment>(), new List<string>());
    }

    public static IAttestationSegmentStore NewCachedAttestationSegmentStore(MemoryAttestationSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationSegmentStore(inner, AttestationSegmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AttestationSegment> FindAttestationSegments(IAttestationSegmentStore store, AttestationSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
