namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional feedback proposals by code.</summary>
public interface IRegionalFeedbackProposalStore
{
    RegionalFeedbackProposal? Find(string code);
    void Save(RegionalFeedbackProposal item);
    bool Remove(string code);
    List<RegionalFeedbackProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalFeedbackProposalStore(Dictionary<string, RegionalFeedbackProposal> items, List<string> order) : IRegionalFeedbackProposalStore
{
    public Dictionary<string, RegionalFeedbackProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalFeedbackProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalFeedbackProposal item)
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

    public List<RegionalFeedbackProposal> List()
    {
        var outValue = new List<RegionalFeedbackProposal>();
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
public sealed class CachedRegionalFeedbackProposalStore(MemoryRegionalFeedbackProposalStore inner, RegionalFeedbackProposalCache cache, int hits, int misses) : IRegionalFeedbackProposalStore
{
    public MemoryRegionalFeedbackProposalStore Inner { get; set; } = inner;
    public RegionalFeedbackProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalFeedbackProposal? Find(string code)
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

    public void Save(RegionalFeedbackProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalFeedbackProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional feedback proposals. Empty fields match all.</summary>
public sealed record RegionalFeedbackProposalQuery(string Channel, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalFeedbackProposal item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalFeedbackProposal> Apply(List<RegionalFeedbackProposal> items)
    {
        var matched = Collections.Filter(items, (RegionalFeedbackProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional feedback proposal store.</summary>
public static class RegionalFeedbackProposalStore
{
    public static IRegionalFeedbackProposalStore NewMemoryRegionalFeedbackProposalStore()
    {
        return new MemoryRegionalFeedbackProposalStore(
            new Dictionary<string, RegionalFeedbackProposal>(),
            new List<string>());
    }

    public static IRegionalFeedbackProposalStore NewCachedRegionalFeedbackProposalStore(MemoryRegionalFeedbackProposalStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalFeedbackProposalStore(
            inner,
            RegionalFeedbackProposalCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<RegionalFeedbackProposal> FindRegionalFeedbackProposals(IRegionalFeedbackProposalStore store, RegionalFeedbackProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
