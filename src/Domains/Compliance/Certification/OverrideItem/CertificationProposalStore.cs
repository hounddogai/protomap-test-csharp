namespace Acme.Workforce.Compliance.Certification.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps certification proposals by code.</summary>
public interface ICertificationProposalStore
{
    CertificationProposal? Find(string code);
    void Save(CertificationProposal item);
    bool Remove(string code);
    List<CertificationProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCertificationProposalStore(Dictionary<string, CertificationProposal> items, List<string> order) : ICertificationProposalStore
{
    public Dictionary<string, CertificationProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CertificationProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CertificationProposal item)
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

    public List<CertificationProposal> List()
    {
        var outValue = new List<CertificationProposal>();
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
public sealed class CachedCertificationProposalStore(MemoryCertificationProposalStore inner, CertificationProposalCache cache, int hits, int misses) : ICertificationProposalStore
{
    public MemoryCertificationProposalStore Inner { get; set; } = inner;
    public CertificationProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CertificationProposal? Find(string code)
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

    public void Save(CertificationProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CertificationProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects certification proposals. Empty fields match all.</summary>
public sealed record CertificationProposalQuery(string Name, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(CertificationProposal item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CertificationProposal> Apply(List<CertificationProposal> items)
    {
        var matched = Collections.Filter(items, (CertificationProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of certification proposal store.</summary>
public static class CertificationProposalStore
{
    public static ICertificationProposalStore NewMemoryCertificationProposalStore()
    {
        return new MemoryCertificationProposalStore(
            new Dictionary<string, CertificationProposal>(),
            new List<string>());
    }

    public static ICertificationProposalStore NewCachedCertificationProposalStore(MemoryCertificationProposalStore inner, int ttl, IClock clock)
    {
        return new CachedCertificationProposalStore(
            inner,
            CertificationProposalCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CertificationProposal> FindCertificationProposals(ICertificationProposalStore store, CertificationProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
