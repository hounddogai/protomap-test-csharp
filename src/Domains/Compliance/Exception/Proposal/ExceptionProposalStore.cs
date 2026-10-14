namespace Acme.Workforce.Compliance.Exception.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception proposals by code.</summary>
public interface IExceptionProposalStore
{
    ExceptionProposal? Find(string code);
    void Save(ExceptionProposal item);
    bool Remove(string code);
    List<ExceptionProposal> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionProposalStore(Dictionary<string, ExceptionProposal> items, List<string> order) : IExceptionProposalStore
{
    public Dictionary<string, ExceptionProposal> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionProposal? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionProposal item)
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

    public List<ExceptionProposal> List()
    {
        var outValue = new List<ExceptionProposal>();
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
public sealed class CachedExceptionProposalStore(MemoryExceptionProposalStore inner, ExceptionProposalCache cache, int hits, int misses) : IExceptionProposalStore
{
    public MemoryExceptionProposalStore Inner { get; set; } = inner;
    public ExceptionProposalCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionProposal? Find(string code)
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

    public void Save(ExceptionProposal item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionProposal> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception proposals. Empty fields match all.</summary>
public sealed record ExceptionProposalQuery(string Department, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionProposal item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ExceptionProposal> Apply(List<ExceptionProposal> items)
    {
        var matched = Collections.Filter(items, (ExceptionProposal candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception proposal store.</summary>
public static class ExceptionProposalStore
{
    public static IExceptionProposalStore NewMemoryExceptionProposalStore()
    {
        return new MemoryExceptionProposalStore(new Dictionary<string, ExceptionProposal>(), new List<string>());
    }

    public static IExceptionProposalStore NewCachedExceptionProposalStore(MemoryExceptionProposalStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionProposalStore(inner, ExceptionProposalCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ExceptionProposal> FindExceptionProposals(IExceptionProposalStore store, ExceptionProposalQuery query)
    {
        return query.Apply(store.List());
    }
}
