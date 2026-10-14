namespace Acme.Workforce.Assets.LicenseSeat.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy license seat proposals by code.</summary>
public interface ILegacyLicenseSeatProposalStore
{
    LegacyLicenseSeatProposal? Find(string code);
    void Save(LegacyLicenseSeatProposal item);
    bool Remove(string code);
    List<LegacyLicenseSeatProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyLicenseSeatProposalStore(Dictionary<string, LegacyLicenseSeatProposal> items, List<string> order) : ILegacyLicenseSeatProposalStore
{
    public Dictionary<string, LegacyLicenseSeatProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyLicenseSeatProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyLicenseSeatProposal item)
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

    public List<LegacyLicenseSeatProposal> List()
    {
        var outValue = new List<LegacyLicenseSeatProposal>();
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
public sealed class CachedLegacyLicenseSeatProposalStore(MemoryLegacyLicenseSeatProposalStore inner, LegacyLicenseSeatProposalCache cache, int hits, int misses) : ILegacyLicenseSeatProposalStore
{
    public MemoryLegacyLicenseSeatProposalStore Inner { get; set; } = inner;
    public LegacyLicenseSeatProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyLicenseSeatProposal? Find(string code)
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

    public void Save(LegacyLicenseSeatProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyLicenseSeatProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy license seat proposals. Empty fields match all.</summary>
public sealed record LegacyLicenseSeatProposalQuery(string Category, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyLicenseSeatProposal item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyLicenseSeatProposal> Apply(List<LegacyLicenseSeatProposal> items)
    {
        var matched = Collections.Filter(items, (LegacyLicenseSeatProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy license seat proposal store.</summary>
public static class LegacyLicenseSeatProposalStore
{
    public static ILegacyLicenseSeatProposalStore NewMemoryLegacyLicenseSeatProposalStore()
    {
        return new MemoryLegacyLicenseSeatProposalStore(
            new Dictionary<string, LegacyLicenseSeatProposal>(),
            new List<string>());
    }

    public static ILegacyLicenseSeatProposalStore NewCachedLegacyLicenseSeatProposalStore(MemoryLegacyLicenseSeatProposalStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyLicenseSeatProposalStore(
            inner,
            LegacyLicenseSeatProposalCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LegacyLicenseSeatProposal> FindLegacyLicenseSeatProposals(ILegacyLicenseSeatProposalStore store, LegacyLicenseSeatProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
