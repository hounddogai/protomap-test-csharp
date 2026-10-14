namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation proposals by code.</summary>
public interface IAttestationProposalStore
{
    AttestationProposal? Find(string code);
    void Save(AttestationProposal item);
    bool Remove(string code);
    List<AttestationProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationProposalStore(Dictionary<string, AttestationProposal> items, List<string> order) : IAttestationProposalStore
{
    public Dictionary<string, AttestationProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationProposal item)
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

    public List<AttestationProposal> List()
    {
        var outValue = new List<AttestationProposal>();
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
public sealed class CachedAttestationProposalStore(MemoryAttestationProposalStore inner, AttestationProposalCache cache, int hits, int misses) : IAttestationProposalStore
{
    public MemoryAttestationProposalStore Inner { get; set; } = inner;
    public AttestationProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationProposal? Find(string code)
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

    public void Save(AttestationProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation proposals. Empty fields match all.</summary>
public sealed record AttestationProposalQuery(string Title, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationProposal item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationProposal> Apply(List<AttestationProposal> items)
    {
        var matched = Collections.Filter(items, (AttestationProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation proposal store.</summary>
public static class AttestationProposalStore
{
    public static IAttestationProposalStore NewMemoryAttestationProposalStore()
    {
        return new MemoryAttestationProposalStore(new Dictionary<string, AttestationProposal>(), new List<string>());
    }

    public static IAttestationProposalStore NewCachedAttestationProposalStore(MemoryAttestationProposalStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationProposalStore(inner, AttestationProposalCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AttestationProposal> FindAttestationProposals(IAttestationProposalStore store, AttestationProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
