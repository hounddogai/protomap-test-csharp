namespace Acme.Workforce.Payrollops.PayRun.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run proposals by code.</summary>
public interface IPayRunProposalStore
{
    PayRunProposal? Find(string code);
    void Save(PayRunProposal item);
    bool Remove(string code);
    List<PayRunProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunProposalStore(Dictionary<string, PayRunProposal> items, List<string> order) : IPayRunProposalStore
{
    public Dictionary<string, PayRunProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunProposal item)
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

    public List<PayRunProposal> List()
    {
        var outValue = new List<PayRunProposal>();
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
public sealed class CachedPayRunProposalStore(MemoryPayRunProposalStore inner, PayRunProposalCache cache, int hits, int misses) : IPayRunProposalStore
{
    public MemoryPayRunProposalStore Inner { get; set; } = inner;
    public PayRunProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunProposal? Find(string code)
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

    public void Save(PayRunProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run proposals. Empty fields match all.</summary>
public sealed record PayRunProposalQuery(string Source, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunProposal item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayRunProposal> Apply(List<PayRunProposal> items)
    {
        var matched = Collections.Filter(items, (PayRunProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run proposal store.</summary>
public static class PayRunProposalStore
{
    public static IPayRunProposalStore NewMemoryPayRunProposalStore()
    {
        return new MemoryPayRunProposalStore(new Dictionary<string, PayRunProposal>(), new List<string>());
    }

    public static IPayRunProposalStore NewCachedPayRunProposalStore(MemoryPayRunProposalStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunProposalStore(inner, PayRunProposalCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PayRunProposal> FindPayRunProposals(IPayRunProposalStore store, PayRunProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
