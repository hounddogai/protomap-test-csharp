namespace Acme.Workforce.Assets.Warranty.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy warranty proposals by code.</summary>
public interface ILegacyWarrantyProposalStore
{
    LegacyWarrantyProposal? Find(string code);
    void Save(LegacyWarrantyProposal item);
    bool Remove(string code);
    List<LegacyWarrantyProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyWarrantyProposalStore(Dictionary<string, LegacyWarrantyProposal> items, List<string> order) : ILegacyWarrantyProposalStore
{
    public Dictionary<string, LegacyWarrantyProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyWarrantyProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyWarrantyProposal item)
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

    public List<LegacyWarrantyProposal> List()
    {
        var outValue = new List<LegacyWarrantyProposal>();
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
public sealed class CachedLegacyWarrantyProposalStore(MemoryLegacyWarrantyProposalStore inner, LegacyWarrantyProposalCache cache, int hits, int misses) : ILegacyWarrantyProposalStore
{
    public MemoryLegacyWarrantyProposalStore Inner { get; set; } = inner;
    public LegacyWarrantyProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyWarrantyProposal? Find(string code)
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

    public void Save(LegacyWarrantyProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyWarrantyProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy warranty proposals. Empty fields match all.</summary>
public sealed record LegacyWarrantyProposalQuery(string Team, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyWarrantyProposal item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<LegacyWarrantyProposal> Apply(List<LegacyWarrantyProposal> items)
    {
        var matched = Collections.Filter(items, (LegacyWarrantyProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy warranty proposal store.</summary>
public static class LegacyWarrantyProposalStore
{
    public static ILegacyWarrantyProposalStore NewMemoryLegacyWarrantyProposalStore()
    {
        return new MemoryLegacyWarrantyProposalStore(
            new Dictionary<string, LegacyWarrantyProposal>(),
            new List<string>());
    }

    public static ILegacyWarrantyProposalStore NewCachedLegacyWarrantyProposalStore(MemoryLegacyWarrantyProposalStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyWarrantyProposalStore(
            inner,
            LegacyWarrantyProposalCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LegacyWarrantyProposal> FindLegacyWarrantyProposals(ILegacyWarrantyProposalStore store, LegacyWarrantyProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
