namespace Acme.Workforce.Compliance.Attestation.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation mappings by code.</summary>
public interface IAttestationMappingStore
{
    AttestationMapping? Find(string code);
    void Save(AttestationMapping item);
    bool Remove(string code);
    List<AttestationMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationMappingStore(Dictionary<string, AttestationMapping> items, List<string> order) : IAttestationMappingStore
{
    public Dictionary<string, AttestationMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationMapping item)
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

    public List<AttestationMapping> List()
    {
        var outValue = new List<AttestationMapping>();
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
public sealed class CachedAttestationMappingStore(MemoryAttestationMappingStore inner, AttestationMappingCache cache, int hits, int misses) : IAttestationMappingStore
{
    public MemoryAttestationMappingStore Inner { get; set; } = inner;
    public AttestationMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationMapping? Find(string code)
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

    public void Save(AttestationMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation mappings. Empty fields match all.</summary>
public sealed record AttestationMappingQuery(string Channel, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationMapping item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationMapping> Apply(List<AttestationMapping> items)
    {
        var matched = Collections.Filter(items, (AttestationMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation mapping store.</summary>
public static class AttestationMappingStore
{
    public static IAttestationMappingStore NewMemoryAttestationMappingStore()
    {
        return new MemoryAttestationMappingStore(new Dictionary<string, AttestationMapping>(), new List<string>());
    }

    public static IAttestationMappingStore NewCachedAttestationMappingStore(MemoryAttestationMappingStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationMappingStore(inner, AttestationMappingCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AttestationMapping> FindAttestationMappings(IAttestationMappingStore store, AttestationMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
