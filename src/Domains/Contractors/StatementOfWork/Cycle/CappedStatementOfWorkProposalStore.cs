namespace Acme.Workforce.Contractors.StatementOfWork.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped statement of work proposals by code.</summary>
public interface ICappedStatementOfWorkProposalStore
{
    CappedStatementOfWorkProposal? Find(string code);
    void Save(CappedStatementOfWorkProposal item);
    bool Remove(string code);
    List<CappedStatementOfWorkProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedStatementOfWorkProposalStore(Dictionary<string, CappedStatementOfWorkProposal> items, List<string> order) : ICappedStatementOfWorkProposalStore
{
    public Dictionary<string, CappedStatementOfWorkProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedStatementOfWorkProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedStatementOfWorkProposal item)
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

    public List<CappedStatementOfWorkProposal> List()
    {
        var outValue = new List<CappedStatementOfWorkProposal>();
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
public sealed class CachedCappedStatementOfWorkProposalStore(MemoryCappedStatementOfWorkProposalStore inner, CappedStatementOfWorkProposalCache cache, int hits, int misses) : ICappedStatementOfWorkProposalStore
{
    public MemoryCappedStatementOfWorkProposalStore Inner { get; set; } = inner;
    public CappedStatementOfWorkProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedStatementOfWorkProposal? Find(string code)
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

    public void Save(CappedStatementOfWorkProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedStatementOfWorkProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped statement of work proposals. Empty fields match all.</summary>
public sealed record CappedStatementOfWorkProposalQuery(string Department, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(CappedStatementOfWorkProposal item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<CappedStatementOfWorkProposal> Apply(List<CappedStatementOfWorkProposal> items)
    {
        var matched = Collections.Filter(items, (CappedStatementOfWorkProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped statement of work proposal store.</summary>
public static class CappedStatementOfWorkProposalStore
{
    public static ICappedStatementOfWorkProposalStore NewMemoryCappedStatementOfWorkProposalStore()
    {
        return new MemoryCappedStatementOfWorkProposalStore(
            new Dictionary<string, CappedStatementOfWorkProposal>(),
            new List<string>());
    }

    public static ICappedStatementOfWorkProposalStore NewCachedCappedStatementOfWorkProposalStore(MemoryCappedStatementOfWorkProposalStore inner, int ttl, IClock clock)
    {
        return new CachedCappedStatementOfWorkProposalStore(
            inner,
            CappedStatementOfWorkProposalCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CappedStatementOfWorkProposal> FindCappedStatementOfWorkProposals(ICappedStatementOfWorkProposalStore store, CappedStatementOfWorkProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
