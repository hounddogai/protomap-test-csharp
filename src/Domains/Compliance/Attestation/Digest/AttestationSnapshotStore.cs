namespace Acme.Workforce.Compliance.Attestation.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation snapshots by code.</summary>
public interface IAttestationSnapshotStore
{
    AttestationSnapshot? Find(string code);
    void Save(AttestationSnapshot item);
    bool Remove(string code);
    List<AttestationSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationSnapshotStore(Dictionary<string, AttestationSnapshot> items, List<string> order) : IAttestationSnapshotStore
{
    public Dictionary<string, AttestationSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationSnapshot item)
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

    public List<AttestationSnapshot> List()
    {
        var outValue = new List<AttestationSnapshot>();
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
public sealed class CachedAttestationSnapshotStore(MemoryAttestationSnapshotStore inner, AttestationSnapshotCache cache, int hits, int misses) : IAttestationSnapshotStore
{
    public MemoryAttestationSnapshotStore Inner { get; set; } = inner;
    public AttestationSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationSnapshot? Find(string code)
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

    public void Save(AttestationSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation snapshots. Empty fields match all.</summary>
public sealed record AttestationSnapshotQuery(string Label, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationSnapshot item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<AttestationSnapshot> Apply(List<AttestationSnapshot> items)
    {
        var matched = Collections.Filter(items, (AttestationSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation snapshot store.</summary>
public static class AttestationSnapshotStore
{
    public static IAttestationSnapshotStore NewMemoryAttestationSnapshotStore()
    {
        return new MemoryAttestationSnapshotStore(new Dictionary<string, AttestationSnapshot>(), new List<string>());
    }

    public static IAttestationSnapshotStore NewCachedAttestationSnapshotStore(MemoryAttestationSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationSnapshotStore(inner, AttestationSnapshotCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AttestationSnapshot> FindAttestationSnapshots(IAttestationSnapshotStore store, AttestationSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
