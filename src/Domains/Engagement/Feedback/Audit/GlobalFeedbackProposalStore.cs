namespace Acme.Workforce.Engagement.Feedback.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global feedback proposals by code.</summary>
public interface IGlobalFeedbackProposalStore
{
    GlobalFeedbackProposal? Find(string code);
    void Save(GlobalFeedbackProposal item);
    bool Remove(string code);
    List<GlobalFeedbackProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalFeedbackProposalStore(Dictionary<string, GlobalFeedbackProposal> items, List<string> order) : IGlobalFeedbackProposalStore
{
    public Dictionary<string, GlobalFeedbackProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalFeedbackProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalFeedbackProposal item)
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

    public List<GlobalFeedbackProposal> List()
    {
        var outValue = new List<GlobalFeedbackProposal>();
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
public sealed class CachedGlobalFeedbackProposalStore(MemoryGlobalFeedbackProposalStore inner, GlobalFeedbackProposalCache cache, int hits, int misses) : IGlobalFeedbackProposalStore
{
    public MemoryGlobalFeedbackProposalStore Inner { get; set; } = inner;
    public GlobalFeedbackProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalFeedbackProposal? Find(string code)
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

    public void Save(GlobalFeedbackProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalFeedbackProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global feedback proposals. Empty fields match all.</summary>
public sealed record GlobalFeedbackProposalQuery(string Category, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalFeedbackProposal item)
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

    public List<GlobalFeedbackProposal> Apply(List<GlobalFeedbackProposal> items)
    {
        var matched = Collections.Filter(items, (GlobalFeedbackProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global feedback proposal store.</summary>
public static class GlobalFeedbackProposalStore
{
    public static IGlobalFeedbackProposalStore NewMemoryGlobalFeedbackProposalStore()
    {
        return new MemoryGlobalFeedbackProposalStore(
            new Dictionary<string, GlobalFeedbackProposal>(),
            new List<string>());
    }

    public static IGlobalFeedbackProposalStore NewCachedGlobalFeedbackProposalStore(MemoryGlobalFeedbackProposalStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalFeedbackProposalStore(
            inner,
            GlobalFeedbackProposalCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<GlobalFeedbackProposal> FindGlobalFeedbackProposals(IGlobalFeedbackProposalStore store, GlobalFeedbackProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
