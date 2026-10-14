namespace Acme.Workforce.Contractors.Rate.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate proposals by code.</summary>
public interface IRateProposalStore
{
    RateProposal? Find(string code);
    void Save(RateProposal item);
    bool Remove(string code);
    List<RateProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateProposalStore(Dictionary<string, RateProposal> items, List<string> order) : IRateProposalStore
{
    public Dictionary<string, RateProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateProposal item)
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

    public List<RateProposal> List()
    {
        var outValue = new List<RateProposal>();
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
public sealed class CachedRateProposalStore(MemoryRateProposalStore inner, RateProposalCache cache, int hits, int misses) : IRateProposalStore
{
    public MemoryRateProposalStore Inner { get; set; } = inner;
    public RateProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateProposal? Find(string code)
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

    public void Save(RateProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate proposals. Empty fields match all.</summary>
public sealed record RateProposalQuery(string Title, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(RateProposal item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RateProposal> Apply(List<RateProposal> items)
    {
        var matched = Collections.Filter(items, (RateProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate proposal store.</summary>
public static class RateProposalStore
{
    public static IRateProposalStore NewMemoryRateProposalStore()
    {
        return new MemoryRateProposalStore(new Dictionary<string, RateProposal>(), new List<string>());
    }

    public static IRateProposalStore NewCachedRateProposalStore(MemoryRateProposalStore inner, int ttl, IClock clock)
    {
        return new CachedRateProposalStore(inner, RateProposalCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RateProposal> FindRateProposals(IRateProposalStore store, RateProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
