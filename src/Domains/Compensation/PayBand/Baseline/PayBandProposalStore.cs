namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band proposals by code.</summary>
public interface IPayBandProposalStore
{
    PayBandProposal? Find(string code);
    void Save(PayBandProposal item);
    bool Remove(string code);
    List<PayBandProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandProposalStore(Dictionary<string, PayBandProposal> items, List<string> order) : IPayBandProposalStore
{
    public Dictionary<string, PayBandProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandProposal item)
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

    public List<PayBandProposal> List()
    {
        var outValue = new List<PayBandProposal>();
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
public sealed class CachedPayBandProposalStore(MemoryPayBandProposalStore inner, PayBandProposalCache cache, int hits, int misses) : IPayBandProposalStore
{
    public MemoryPayBandProposalStore Inner { get; set; } = inner;
    public PayBandProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandProposal? Find(string code)
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

    public void Save(PayBandProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band proposals. Empty fields match all.</summary>
public sealed record PayBandProposalQuery(string Title, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandProposal item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandProposal> Apply(List<PayBandProposal> items)
    {
        var matched = Collections.Filter(items, (PayBandProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band proposal store.</summary>
public static class PayBandProposalStore
{
    public static IPayBandProposalStore NewMemoryPayBandProposalStore()
    {
        return new MemoryPayBandProposalStore(new Dictionary<string, PayBandProposal>(), new List<string>());
    }

    public static IPayBandProposalStore NewCachedPayBandProposalStore(MemoryPayBandProposalStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandProposalStore(inner, PayBandProposalCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PayBandProposal> FindPayBandProposals(IPayBandProposalStore store, PayBandProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
