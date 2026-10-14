namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work proposals by code.</summary>
public interface IStatementOfWorkProposalStore
{
    StatementOfWorkProposal? Find(string code);
    void Save(StatementOfWorkProposal item);
    bool Remove(string code);
    List<StatementOfWorkProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkProposalStore(Dictionary<string, StatementOfWorkProposal> items, List<string> order) : IStatementOfWorkProposalStore
{
    public Dictionary<string, StatementOfWorkProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkProposal item)
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

    public List<StatementOfWorkProposal> List()
    {
        var outValue = new List<StatementOfWorkProposal>();
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
public sealed class CachedStatementOfWorkProposalStore(MemoryStatementOfWorkProposalStore inner, StatementOfWorkProposalCache cache, int hits, int misses) : IStatementOfWorkProposalStore
{
    public MemoryStatementOfWorkProposalStore Inner { get; set; } = inner;
    public StatementOfWorkProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkProposal? Find(string code)
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

    public void Save(StatementOfWorkProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work proposals. Empty fields match all.</summary>
public sealed record StatementOfWorkProposalQuery(string Title, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkProposal item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkProposal> Apply(List<StatementOfWorkProposal> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work proposal store.</summary>
public static class StatementOfWorkProposalStore
{
    public static IStatementOfWorkProposalStore NewMemoryStatementOfWorkProposalStore()
    {
        return new MemoryStatementOfWorkProposalStore(
            new Dictionary<string, StatementOfWorkProposal>(),
            new List<string>());
    }

    public static IStatementOfWorkProposalStore NewCachedStatementOfWorkProposalStore(MemoryStatementOfWorkProposalStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkProposalStore(
            inner,
            StatementOfWorkProposalCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<StatementOfWorkProposal> FindStatementOfWorkProposals(IStatementOfWorkProposalStore store, StatementOfWorkProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
