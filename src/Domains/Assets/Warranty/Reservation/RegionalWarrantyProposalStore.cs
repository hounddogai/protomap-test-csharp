namespace Acme.Workforce.Assets.Warranty.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional warranty proposals by code.</summary>
public interface IRegionalWarrantyProposalStore
{
    RegionalWarrantyProposal? Find(string code);
    void Save(RegionalWarrantyProposal item);
    bool Remove(string code);
    List<RegionalWarrantyProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalWarrantyProposalStore(Dictionary<string, RegionalWarrantyProposal> items, List<string> order) : IRegionalWarrantyProposalStore
{
    public Dictionary<string, RegionalWarrantyProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalWarrantyProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalWarrantyProposal item)
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

    public List<RegionalWarrantyProposal> List()
    {
        var outValue = new List<RegionalWarrantyProposal>();
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
public sealed class CachedRegionalWarrantyProposalStore(MemoryRegionalWarrantyProposalStore inner, RegionalWarrantyProposalCache cache, int hits, int misses) : IRegionalWarrantyProposalStore
{
    public MemoryRegionalWarrantyProposalStore Inner { get; set; } = inner;
    public RegionalWarrantyProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalWarrantyProposal? Find(string code)
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

    public void Save(RegionalWarrantyProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalWarrantyProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional warranty proposals. Empty fields match all.</summary>
public sealed record RegionalWarrantyProposalQuery(string Source, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalWarrantyProposal item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalWarrantyProposal> Apply(List<RegionalWarrantyProposal> items)
    {
        var matched = Collections.Filter(items, (RegionalWarrantyProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional warranty proposal store.</summary>
public static class RegionalWarrantyProposalStore
{
    public static IRegionalWarrantyProposalStore NewMemoryRegionalWarrantyProposalStore()
    {
        return new MemoryRegionalWarrantyProposalStore(
            new Dictionary<string, RegionalWarrantyProposal>(),
            new List<string>());
    }

    public static IRegionalWarrantyProposalStore NewCachedRegionalWarrantyProposalStore(MemoryRegionalWarrantyProposalStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalWarrantyProposalStore(
            inner,
            RegionalWarrantyProposalCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RegionalWarrantyProposal> FindRegionalWarrantyProposals(IRegionalWarrantyProposalStore store, RegionalWarrantyProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
