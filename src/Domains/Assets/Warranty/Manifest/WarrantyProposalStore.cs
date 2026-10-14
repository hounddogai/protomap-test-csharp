namespace Acme.Workforce.Assets.Warranty.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty proposals by code.</summary>
public interface IWarrantyProposalStore
{
    WarrantyProposal? Find(string code);
    void Save(WarrantyProposal item);
    bool Remove(string code);
    List<WarrantyProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyProposalStore(Dictionary<string, WarrantyProposal> items, List<string> order) : IWarrantyProposalStore
{
    public Dictionary<string, WarrantyProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyProposal item)
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

    public List<WarrantyProposal> List()
    {
        var outValue = new List<WarrantyProposal>();
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
public sealed class CachedWarrantyProposalStore(MemoryWarrantyProposalStore inner, WarrantyProposalCache cache, int hits, int misses) : IWarrantyProposalStore
{
    public MemoryWarrantyProposalStore Inner { get; set; } = inner;
    public WarrantyProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyProposal? Find(string code)
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

    public void Save(WarrantyProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty proposals. Empty fields match all.</summary>
public sealed record WarrantyProposalQuery(string Category, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyProposal item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyProposal> Apply(List<WarrantyProposal> items)
    {
        var matched = Collections.Filter(items, (WarrantyProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty proposal store.</summary>
public static class WarrantyProposalStore
{
    public static IWarrantyProposalStore NewMemoryWarrantyProposalStore()
    {
        return new MemoryWarrantyProposalStore(new Dictionary<string, WarrantyProposal>(), new List<string>());
    }

    public static IWarrantyProposalStore NewCachedWarrantyProposalStore(MemoryWarrantyProposalStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyProposalStore(inner, WarrantyProposalCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<WarrantyProposal> FindWarrantyProposals(IWarrantyProposalStore store, WarrantyProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
