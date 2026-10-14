namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor proposals by code.</summary>
public interface IVendorProposalStore
{
    VendorProposal? Find(string code);
    void Save(VendorProposal item);
    bool Remove(string code);
    List<VendorProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorProposalStore(Dictionary<string, VendorProposal> items, List<string> order) : IVendorProposalStore
{
    public Dictionary<string, VendorProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorProposal item)
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

    public List<VendorProposal> List()
    {
        var outValue = new List<VendorProposal>();
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
public sealed class CachedVendorProposalStore(MemoryVendorProposalStore inner, VendorProposalCache cache, int hits, int misses) : IVendorProposalStore
{
    public MemoryVendorProposalStore Inner { get; set; } = inner;
    public VendorProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorProposal? Find(string code)
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

    public void Save(VendorProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor proposals. Empty fields match all.</summary>
public sealed record VendorProposalQuery(string Label, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(VendorProposal item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VendorProposal> Apply(List<VendorProposal> items)
    {
        var matched = Collections.Filter(items, (VendorProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor proposal store.</summary>
public static class VendorProposalStore
{
    public static IVendorProposalStore NewMemoryVendorProposalStore()
    {
        return new MemoryVendorProposalStore(new Dictionary<string, VendorProposal>(), new List<string>());
    }

    public static IVendorProposalStore NewCachedVendorProposalStore(MemoryVendorProposalStore inner, int ttl, IClock clock)
    {
        return new CachedVendorProposalStore(inner, VendorProposalCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<VendorProposal> FindVendorProposals(IVendorProposalStore store, VendorProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
