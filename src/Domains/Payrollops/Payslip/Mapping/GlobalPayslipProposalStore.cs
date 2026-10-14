namespace Acme.Workforce.Payrollops.Payslip.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global payslip proposals by code.</summary>
public interface IGlobalPayslipProposalStore
{
    GlobalPayslipProposal? Find(string code);
    void Save(GlobalPayslipProposal item);
    bool Remove(string code);
    List<GlobalPayslipProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalPayslipProposalStore(Dictionary<string, GlobalPayslipProposal> items, List<string> order) : IGlobalPayslipProposalStore
{
    public Dictionary<string, GlobalPayslipProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalPayslipProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalPayslipProposal item)
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

    public List<GlobalPayslipProposal> List()
    {
        var outValue = new List<GlobalPayslipProposal>();
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
public sealed class CachedGlobalPayslipProposalStore(MemoryGlobalPayslipProposalStore inner, GlobalPayslipProposalCache cache, int hits, int misses) : IGlobalPayslipProposalStore
{
    public MemoryGlobalPayslipProposalStore Inner { get; set; } = inner;
    public GlobalPayslipProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalPayslipProposal? Find(string code)
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

    public void Save(GlobalPayslipProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalPayslipProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global payslip proposals. Empty fields match all.</summary>
public sealed record GlobalPayslipProposalQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalPayslipProposal item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalPayslipProposal> Apply(List<GlobalPayslipProposal> items)
    {
        var matched = Collections.Filter(items, (GlobalPayslipProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global payslip proposal store.</summary>
public static class GlobalPayslipProposalStore
{
    public static IGlobalPayslipProposalStore NewMemoryGlobalPayslipProposalStore()
    {
        return new MemoryGlobalPayslipProposalStore(
            new Dictionary<string, GlobalPayslipProposal>(),
            new List<string>());
    }

    public static IGlobalPayslipProposalStore NewCachedGlobalPayslipProposalStore(MemoryGlobalPayslipProposalStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalPayslipProposalStore(inner, GlobalPayslipProposalCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<GlobalPayslipProposal> FindGlobalPayslipProposals(IGlobalPayslipProposalStore store, GlobalPayslipProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
