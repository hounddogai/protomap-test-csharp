namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered feedback proposals by code.</summary>
public interface ITieredFeedbackProposalStore
{
    TieredFeedbackProposal? Find(string code);
    void Save(TieredFeedbackProposal item);
    bool Remove(string code);
    List<TieredFeedbackProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredFeedbackProposalStore(Dictionary<string, TieredFeedbackProposal> items, List<string> order) : ITieredFeedbackProposalStore
{
    public Dictionary<string, TieredFeedbackProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredFeedbackProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredFeedbackProposal item)
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

    public List<TieredFeedbackProposal> List()
    {
        var outValue = new List<TieredFeedbackProposal>();
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
public sealed class CachedTieredFeedbackProposalStore(MemoryTieredFeedbackProposalStore inner, TieredFeedbackProposalCache cache, int hits, int misses) : ITieredFeedbackProposalStore
{
    public MemoryTieredFeedbackProposalStore Inner { get; set; } = inner;
    public TieredFeedbackProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredFeedbackProposal? Find(string code)
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

    public void Save(TieredFeedbackProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredFeedbackProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered feedback proposals. Empty fields match all.</summary>
public sealed record TieredFeedbackProposalQuery(string Segment, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(TieredFeedbackProposal item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<TieredFeedbackProposal> Apply(List<TieredFeedbackProposal> items)
    {
        var matched = Collections.Filter(items, (TieredFeedbackProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered feedback proposal store.</summary>
public static class TieredFeedbackProposalStore
{
    public static ITieredFeedbackProposalStore NewMemoryTieredFeedbackProposalStore()
    {
        return new MemoryTieredFeedbackProposalStore(
            new Dictionary<string, TieredFeedbackProposal>(),
            new List<string>());
    }

    public static ITieredFeedbackProposalStore NewCachedTieredFeedbackProposalStore(MemoryTieredFeedbackProposalStore inner, int ttl, IClock clock)
    {
        return new CachedTieredFeedbackProposalStore(
            inner,
            TieredFeedbackProposalCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<TieredFeedbackProposal> FindTieredFeedbackProposals(ITieredFeedbackProposalStore store, TieredFeedbackProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
