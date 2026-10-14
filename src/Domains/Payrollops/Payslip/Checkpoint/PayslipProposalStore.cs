namespace Acme.Workforce.Payrollops.Payslip.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip proposals by code.</summary>
public interface IPayslipProposalStore
{
    PayslipProposal? Find(string code);
    void Save(PayslipProposal item);
    bool Remove(string code);
    List<PayslipProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipProposalStore(Dictionary<string, PayslipProposal> items, List<string> order) : IPayslipProposalStore
{
    public Dictionary<string, PayslipProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipProposal item)
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

    public List<PayslipProposal> List()
    {
        var outValue = new List<PayslipProposal>();
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
public sealed class CachedPayslipProposalStore(MemoryPayslipProposalStore inner, PayslipProposalCache cache, int hits, int misses) : IPayslipProposalStore
{
    public MemoryPayslipProposalStore Inner { get; set; } = inner;
    public PayslipProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipProposal? Find(string code)
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

    public void Save(PayslipProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip proposals. Empty fields match all.</summary>
public sealed record PayslipProposalQuery(string Title, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipProposal item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<PayslipProposal> Apply(List<PayslipProposal> items)
    {
        var matched = Collections.Filter(items, (PayslipProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip proposal store.</summary>
public static class PayslipProposalStore
{
    public static IPayslipProposalStore NewMemoryPayslipProposalStore()
    {
        return new MemoryPayslipProposalStore(new Dictionary<string, PayslipProposal>(), new List<string>());
    }

    public static IPayslipProposalStore NewCachedPayslipProposalStore(MemoryPayslipProposalStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipProposalStore(inner, PayslipProposalCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PayslipProposal> FindPayslipProposals(IPayslipProposalStore store, PayslipProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
