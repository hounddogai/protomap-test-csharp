namespace Acme.Workforce.Compliance.Attestation.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation manifests by code.</summary>
public interface IAttestationManifestStore
{
    AttestationManifest? Find(string code);
    void Save(AttestationManifest item);
    bool Remove(string code);
    List<AttestationManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationManifestStore(Dictionary<string, AttestationManifest> items, List<string> order) : IAttestationManifestStore
{
    public Dictionary<string, AttestationManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationManifest item)
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

    public List<AttestationManifest> List()
    {
        var outValue = new List<AttestationManifest>();
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
public sealed class CachedAttestationManifestStore(MemoryAttestationManifestStore inner, AttestationManifestCache cache, int hits, int misses) : IAttestationManifestStore
{
    public MemoryAttestationManifestStore Inner { get; set; } = inner;
    public AttestationManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationManifest? Find(string code)
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

    public void Save(AttestationManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation manifests. Empty fields match all.</summary>
public sealed record AttestationManifestQuery(string Label, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationManifest item)
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

    public List<AttestationManifest> Apply(List<AttestationManifest> items)
    {
        var matched = Collections.Filter(items, (AttestationManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation manifest store.</summary>
public static class AttestationManifestStore
{
    public static IAttestationManifestStore NewMemoryAttestationManifestStore()
    {
        return new MemoryAttestationManifestStore(new Dictionary<string, AttestationManifest>(), new List<string>());
    }

    public static IAttestationManifestStore NewCachedAttestationManifestStore(MemoryAttestationManifestStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationManifestStore(inner, AttestationManifestCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AttestationManifest> FindAttestationManifests(IAttestationManifestStore store, AttestationManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
