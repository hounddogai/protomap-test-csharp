namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool proposals by code.</summary>
public interface IBonusPoolProposalStore
{
    BonusPoolProposal? Find(string code);
    void Save(BonusPoolProposal item);
    bool Remove(string code);
    List<BonusPoolProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolProposalStore(Dictionary<string, BonusPoolProposal> items, List<string> order) : IBonusPoolProposalStore
{
    public Dictionary<string, BonusPoolProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolProposal item)
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

    public List<BonusPoolProposal> List()
    {
        var outValue = new List<BonusPoolProposal>();
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
public sealed class CachedBonusPoolProposalStore(MemoryBonusPoolProposalStore inner, BonusPoolProposalCache cache, int hits, int misses) : IBonusPoolProposalStore
{
    public MemoryBonusPoolProposalStore Inner { get; set; } = inner;
    public BonusPoolProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolProposal? Find(string code)
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

    public void Save(BonusPoolProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool proposals. Empty fields match all.</summary>
public sealed record BonusPoolProposalQuery(string Title, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolProposal item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolProposal> Apply(List<BonusPoolProposal> items)
    {
        var matched = Collections.Filter(items, (BonusPoolProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool proposal store.</summary>
public static class BonusPoolProposalStore
{
    public static IBonusPoolProposalStore NewMemoryBonusPoolProposalStore()
    {
        return new MemoryBonusPoolProposalStore(new Dictionary<string, BonusPoolProposal>(), new List<string>());
    }

    public static IBonusPoolProposalStore NewCachedBonusPoolProposalStore(MemoryBonusPoolProposalStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolProposalStore(inner, BonusPoolProposalCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BonusPoolProposal> FindBonusPoolProposals(IBonusPoolProposalStore store, BonusPoolProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
