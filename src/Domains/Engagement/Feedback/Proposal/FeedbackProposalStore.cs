namespace Acme.Workforce.Engagement.Feedback.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback proposals by code.</summary>
public interface IFeedbackProposalStore
{
    FeedbackProposal? Find(string code);
    void Save(FeedbackProposal item);
    bool Remove(string code);
    List<FeedbackProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackProposalStore(Dictionary<string, FeedbackProposal> items, List<string> order) : IFeedbackProposalStore
{
    public Dictionary<string, FeedbackProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackProposal item)
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

    public List<FeedbackProposal> List()
    {
        var outValue = new List<FeedbackProposal>();
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
public sealed class CachedFeedbackProposalStore(MemoryFeedbackProposalStore inner, FeedbackProposalCache cache, int hits, int misses) : IFeedbackProposalStore
{
    public MemoryFeedbackProposalStore Inner { get; set; } = inner;
    public FeedbackProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackProposal? Find(string code)
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

    public void Save(FeedbackProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback proposals. Empty fields match all.</summary>
public sealed record FeedbackProposalQuery(string Name, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackProposal item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FeedbackProposal> Apply(List<FeedbackProposal> items)
    {
        var matched = Collections.Filter(items, (FeedbackProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback proposal store.</summary>
public static class FeedbackProposalStore
{
    public static IFeedbackProposalStore NewMemoryFeedbackProposalStore()
    {
        return new MemoryFeedbackProposalStore(new Dictionary<string, FeedbackProposal>(), new List<string>());
    }

    public static IFeedbackProposalStore NewCachedFeedbackProposalStore(MemoryFeedbackProposalStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackProposalStore(inner, FeedbackProposalCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<FeedbackProposal> FindFeedbackProposals(IFeedbackProposalStore store, FeedbackProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
