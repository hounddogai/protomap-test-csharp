namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation requests by code.</summary>
public interface IAttestationRequestStore
{
    AttestationRequest? Find(string code);
    void Save(AttestationRequest item);
    bool Remove(string code);
    List<AttestationRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationRequestStore(Dictionary<string, AttestationRequest> items, List<string> order) : IAttestationRequestStore
{
    public Dictionary<string, AttestationRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationRequest item)
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

    public List<AttestationRequest> List()
    {
        var outValue = new List<AttestationRequest>();
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
public sealed class CachedAttestationRequestStore(MemoryAttestationRequestStore inner, AttestationRequestCache cache, int hits, int misses) : IAttestationRequestStore
{
    public MemoryAttestationRequestStore Inner { get; set; } = inner;
    public AttestationRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationRequest? Find(string code)
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

    public void Save(AttestationRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation requests. Empty fields match all.</summary>
public sealed record AttestationRequestQuery(string Team, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationRequest item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationRequest> Apply(List<AttestationRequest> items)
    {
        var matched = Collections.Filter(items, (AttestationRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation request store.</summary>
public static class AttestationRequestStore
{
    public static IAttestationRequestStore NewMemoryAttestationRequestStore()
    {
        return new MemoryAttestationRequestStore(new Dictionary<string, AttestationRequest>(), new List<string>());
    }

    public static IAttestationRequestStore NewCachedAttestationRequestStore(MemoryAttestationRequestStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationRequestStore(inner, AttestationRequestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AttestationRequest> FindAttestationRequests(IAttestationRequestStore store, AttestationRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
