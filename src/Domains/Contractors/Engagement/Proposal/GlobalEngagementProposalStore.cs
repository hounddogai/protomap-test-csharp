namespace Acme.Workforce.Contractors.Engagement.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global engagement proposals by code.</summary>
public interface IGlobalEngagementProposalStore
{
    GlobalEngagementProposal? Find(string code);
    void Save(GlobalEngagementProposal item);
    bool Remove(string code);
    List<GlobalEngagementProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalEngagementProposalStore(Dictionary<string, GlobalEngagementProposal> items, List<string> order) : IGlobalEngagementProposalStore
{
    public Dictionary<string, GlobalEngagementProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalEngagementProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalEngagementProposal item)
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

    public List<GlobalEngagementProposal> List()
    {
        var outValue = new List<GlobalEngagementProposal>();
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
public sealed class CachedGlobalEngagementProposalStore(MemoryGlobalEngagementProposalStore inner, GlobalEngagementProposalCache cache, int hits, int misses) : IGlobalEngagementProposalStore
{
    public MemoryGlobalEngagementProposalStore Inner { get; set; } = inner;
    public GlobalEngagementProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalEngagementProposal? Find(string code)
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

    public void Save(GlobalEngagementProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalEngagementProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global engagement proposals. Empty fields match all.</summary>
public sealed record GlobalEngagementProposalQuery(string Name, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalEngagementProposal item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalEngagementProposal> Apply(List<GlobalEngagementProposal> items)
    {
        var matched = Collections.Filter(items, (GlobalEngagementProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global engagement proposal store.</summary>
public static class GlobalEngagementProposalStore
{
    public static IGlobalEngagementProposalStore NewMemoryGlobalEngagementProposalStore()
    {
        return new MemoryGlobalEngagementProposalStore(
            new Dictionary<string, GlobalEngagementProposal>(),
            new List<string>());
    }

    public static IGlobalEngagementProposalStore NewCachedGlobalEngagementProposalStore(MemoryGlobalEngagementProposalStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalEngagementProposalStore(
            inner,
            GlobalEngagementProposalCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<GlobalEngagementProposal> FindGlobalEngagementProposals(IGlobalEngagementProposalStore store, GlobalEngagementProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
