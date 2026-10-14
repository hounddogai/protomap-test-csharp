namespace Acme.Workforce.Contractors.Rate.RateNotice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped rate proposals by code.</summary>
public interface ICappedRateProposalStore
{
    CappedRateProposal? Find(string code);
    void Save(CappedRateProposal item);
    bool Remove(string code);
    List<CappedRateProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedRateProposalStore(Dictionary<string, CappedRateProposal> items, List<string> order) : ICappedRateProposalStore
{
    public Dictionary<string, CappedRateProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedRateProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedRateProposal item)
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

    public List<CappedRateProposal> List()
    {
        var outValue = new List<CappedRateProposal>();
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
public sealed class CachedCappedRateProposalStore(MemoryCappedRateProposalStore inner, CappedRateProposalCache cache, int hits, int misses) : ICappedRateProposalStore
{
    public MemoryCappedRateProposalStore Inner { get; set; } = inner;
    public CappedRateProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedRateProposal? Find(string code)
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

    public void Save(CappedRateProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedRateProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped rate proposals. Empty fields match all.</summary>
public sealed record CappedRateProposalQuery(string Name, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(CappedRateProposal item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedRateProposal> Apply(List<CappedRateProposal> items)
    {
        var matched = Collections.Filter(items, (CappedRateProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped rate proposal store.</summary>
public static class CappedRateProposalStore
{
    public static ICappedRateProposalStore NewMemoryCappedRateProposalStore()
    {
        return new MemoryCappedRateProposalStore(new Dictionary<string, CappedRateProposal>(), new List<string>());
    }

    public static ICappedRateProposalStore NewCachedCappedRateProposalStore(MemoryCappedRateProposalStore inner, int ttl, IClock clock)
    {
        return new CachedCappedRateProposalStore(inner, CappedRateProposalCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<CappedRateProposal> FindCappedRateProposals(ICappedRateProposalStore store, CappedRateProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
