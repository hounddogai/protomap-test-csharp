namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy bonus pool proposals by code.</summary>
public interface ILegacyBonusPoolProposalStore
{
    LegacyBonusPoolProposal? Find(string code);
    void Save(LegacyBonusPoolProposal item);
    bool Remove(string code);
    List<LegacyBonusPoolProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyBonusPoolProposalStore(Dictionary<string, LegacyBonusPoolProposal> items, List<string> order) : ILegacyBonusPoolProposalStore
{
    public Dictionary<string, LegacyBonusPoolProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyBonusPoolProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyBonusPoolProposal item)
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

    public List<LegacyBonusPoolProposal> List()
    {
        var outValue = new List<LegacyBonusPoolProposal>();
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
public sealed class CachedLegacyBonusPoolProposalStore(MemoryLegacyBonusPoolProposalStore inner, LegacyBonusPoolProposalCache cache, int hits, int misses) : ILegacyBonusPoolProposalStore
{
    public MemoryLegacyBonusPoolProposalStore Inner { get; set; } = inner;
    public LegacyBonusPoolProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyBonusPoolProposal? Find(string code)
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

    public void Save(LegacyBonusPoolProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyBonusPoolProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy bonus pool proposals. Empty fields match all.</summary>
public sealed record LegacyBonusPoolProposalQuery(string Region, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyBonusPoolProposal item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyBonusPoolProposal> Apply(List<LegacyBonusPoolProposal> items)
    {
        var matched = Collections.Filter(items, (LegacyBonusPoolProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy bonus pool proposal store.</summary>
public static class LegacyBonusPoolProposalStore
{
    public static ILegacyBonusPoolProposalStore NewMemoryLegacyBonusPoolProposalStore()
    {
        return new MemoryLegacyBonusPoolProposalStore(
            new Dictionary<string, LegacyBonusPoolProposal>(),
            new List<string>());
    }

    public static ILegacyBonusPoolProposalStore NewCachedLegacyBonusPoolProposalStore(MemoryLegacyBonusPoolProposalStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyBonusPoolProposalStore(
            inner,
            LegacyBonusPoolProposalCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LegacyBonusPoolProposal> FindLegacyBonusPoolProposals(ILegacyBonusPoolProposalStore store, LegacyBonusPoolProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
