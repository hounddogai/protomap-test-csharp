namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict retro pay proposals by code.</summary>
public interface IStrictRetroPayProposalStore
{
    StrictRetroPayProposal? Find(string code);
    void Save(StrictRetroPayProposal item);
    bool Remove(string code);
    List<StrictRetroPayProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictRetroPayProposalStore(Dictionary<string, StrictRetroPayProposal> items, List<string> order) : IStrictRetroPayProposalStore
{
    public Dictionary<string, StrictRetroPayProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictRetroPayProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictRetroPayProposal item)
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

    public List<StrictRetroPayProposal> List()
    {
        var outValue = new List<StrictRetroPayProposal>();
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
public sealed class CachedStrictRetroPayProposalStore(MemoryStrictRetroPayProposalStore inner, StrictRetroPayProposalCache cache, int hits, int misses) : IStrictRetroPayProposalStore
{
    public MemoryStrictRetroPayProposalStore Inner { get; set; } = inner;
    public StrictRetroPayProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictRetroPayProposal? Find(string code)
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

    public void Save(StrictRetroPayProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictRetroPayProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict retro pay proposals. Empty fields match all.</summary>
public sealed record StrictRetroPayProposalQuery(string Category, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(StrictRetroPayProposal item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<StrictRetroPayProposal> Apply(List<StrictRetroPayProposal> items)
    {
        var matched = Collections.Filter(items, (StrictRetroPayProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict retro pay proposal store.</summary>
public static class StrictRetroPayProposalStore
{
    public static IStrictRetroPayProposalStore NewMemoryStrictRetroPayProposalStore()
    {
        return new MemoryStrictRetroPayProposalStore(
            new Dictionary<string, StrictRetroPayProposal>(),
            new List<string>());
    }

    public static IStrictRetroPayProposalStore NewCachedStrictRetroPayProposalStore(MemoryStrictRetroPayProposalStore inner, int ttl, IClock clock)
    {
        return new CachedStrictRetroPayProposalStore(
            inner,
            StrictRetroPayProposalCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<StrictRetroPayProposal> FindStrictRetroPayProposals(IStrictRetroPayProposalStore store, StrictRetroPayProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
