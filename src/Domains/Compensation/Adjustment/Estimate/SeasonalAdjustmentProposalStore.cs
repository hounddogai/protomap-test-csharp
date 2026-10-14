namespace Acme.Workforce.Compensation.Adjustment.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal adjustment proposals by code.</summary>
public interface ISeasonalAdjustmentProposalStore
{
    SeasonalAdjustmentProposal? Find(string code);
    void Save(SeasonalAdjustmentProposal item);
    bool Remove(string code);
    List<SeasonalAdjustmentProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalAdjustmentProposalStore(Dictionary<string, SeasonalAdjustmentProposal> items, List<string> order) : ISeasonalAdjustmentProposalStore
{
    public Dictionary<string, SeasonalAdjustmentProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalAdjustmentProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalAdjustmentProposal item)
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

    public List<SeasonalAdjustmentProposal> List()
    {
        var outValue = new List<SeasonalAdjustmentProposal>();
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
public sealed class CachedSeasonalAdjustmentProposalStore(MemorySeasonalAdjustmentProposalStore inner, SeasonalAdjustmentProposalCache cache, int hits, int misses) : ISeasonalAdjustmentProposalStore
{
    public MemorySeasonalAdjustmentProposalStore Inner { get; set; } = inner;
    public SeasonalAdjustmentProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalAdjustmentProposal? Find(string code)
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

    public void Save(SeasonalAdjustmentProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalAdjustmentProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal adjustment proposals. Empty fields match all.</summary>
public sealed record SeasonalAdjustmentProposalQuery(string Label, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalAdjustmentProposal item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalAdjustmentProposal> Apply(List<SeasonalAdjustmentProposal> items)
    {
        var matched = Collections.Filter(items, (SeasonalAdjustmentProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal adjustment proposal store.</summary>
public static class SeasonalAdjustmentProposalStore
{
    public static ISeasonalAdjustmentProposalStore NewMemorySeasonalAdjustmentProposalStore()
    {
        return new MemorySeasonalAdjustmentProposalStore(
            new Dictionary<string, SeasonalAdjustmentProposal>(),
            new List<string>());
    }

    public static ISeasonalAdjustmentProposalStore NewCachedSeasonalAdjustmentProposalStore(MemorySeasonalAdjustmentProposalStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalAdjustmentProposalStore(
            inner,
            SeasonalAdjustmentProposalCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalAdjustmentProposal> FindSeasonalAdjustmentProposals(ISeasonalAdjustmentProposalStore store, SeasonalAdjustmentProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
